using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class TopicExtensionsTests
    {
        private readonly ITestOutputHelper log;
        private readonly IConfiguration cfg;

        private readonly string connectionString;
        private readonly ManagementClient managementClient;

        public TopicExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;
            this.cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementClient = new ManagementClient(connectionString);
        }

        [Fact(Skip = "Called directly from receive")]
        public async Task TestTopicSend()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<TopicClient>(sp => new TopicClient(connectionString, "mediator-topic"))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTopicOptions<EchoRequest, EchoResponse>()
                .AddSendTopicMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact]
        public async Task TestSubscriptionReceive()
        {
            await TestTopicSend();

            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<SubscriptionClient>(sp => new SubscriptionClient(connectionString, "mediator-topic", "mediator-sub"))
                .AddTransient<ITestOutputHelper>(sp => log)

                .BuildServiceProvider();

            var subscriptionClient = serviceProvider.GetRequiredService<SubscriptionClient>();

            var messageHandler = new Func<Message, CancellationToken, Task>((msg, tkn) =>
            {
                log.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} - Received message {msg.MessageId}");

                return Task.CompletedTask;
            });

            var exceptionHandler = new Func<ExceptionReceivedEventArgs, Task>((args) =>
            {
                log.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} - Received exception {args.Exception.Message}");

                return Task.CompletedTask;
            });

            subscriptionClient.RegisterMessageHandler(messageHandler, exceptionHandler);

            // command implementation - token is received from mediator
            using var cancelSource = new CancellationTokenSource(3000);

            var receivePolicy = Policy
                .HandleResult<CancellationToken>(tkn => tkn.IsCancellationRequested == false)
                .WaitAndRetryForever(i => TimeSpan.FromMilliseconds(500));

            receivePolicy.Execute(() =>
            {
                log.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} - Executing receive policy");

                return cancelSource.Token;
            });
        }
    }
}
