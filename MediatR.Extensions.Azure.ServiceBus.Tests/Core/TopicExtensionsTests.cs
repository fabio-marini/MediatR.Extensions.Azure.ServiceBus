using FluentAssertions;
using MediatR.Extensions.Azure.Storage.Examples;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests.Core
{
    [Trait("TestCategory", "Integration"), Collection("TopicTests")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class TopicExtensionsTests
    {
        private readonly ITestOutputHelper log;
        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        public TopicExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ManagementClient(connectionString));
        }

        [Fact(DisplayName = "01. Topic and subscriptions are recreated")]
        public async Task Step01() => await managementFixture.TopicIsRecreated(TestEntities.TopicPath, TestEntities.SubscriptionName);

        [Fact(DisplayName = "02. Send extensions are executed")]
        public async Task Step02()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<MessageSender>(sp => new MessageSender(connectionString, TestEntities.TopicPath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions { MinimumLogLevel = LogLevel.Information })
                .AddMessageOptions()
                .AddSendMessageExtensions()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact(DisplayName = "03. Subscriptions have messages")]
        public async Task Step03() => await managementFixture.SubscriptionHasMessages(TestEntities.TopicPath, TestEntities.SubscriptionName, 4);

        [Fact(DisplayName = "04. Receive extensions are executed")]
        public async Task Step04()
        {
            var entityPath = EntityNameHelper.FormatSubscriptionPath(TestEntities.TopicPath, TestEntities.SubscriptionName);

            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<MessageReceiver>(sp => new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions { MinimumLogLevel = LogLevel.Information })
                .AddMessageOptions()
                .AddReceiveMessageExtensions()

                .BuildServiceProvider();

            using var cancelSource = new CancellationTokenSource(3000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact(DisplayName = "05. Subscriptions have messages")]
        public async Task Step05() => await managementFixture.SubscriptionHasMessages(TestEntities.TopicPath, TestEntities.SubscriptionName, 0);
    }
}
