using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class TestSubscriptions
    {
        public const string RequestProcessor = "request-processor";
        public const string ResponseProcessor = "response-processor";
        public const string RequestBehavior = "request-behavior";
        public const string ResponseBehavior = "response-behavior";
    }

    [Trait("TestCategory", "Integration")]
    [TestCaseOrderer("Timeless.Testing.Xunit.TestMethodNameOrderer", "MediatR.Extensions.Azure.ServiceBus.Tests")]
    public class TopicExtensionsTests
    {
        private readonly ITestOutputHelper log;
        private readonly IConfiguration cfg;

        private readonly string connectionString;
        private readonly ManagementClient managementClient;

        private const string topicPath = "mediator-topic";

        public TopicExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;
            this.cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementClient = new ManagementClient(connectionString);
        }

        public static IEnumerable<object[]> QueueNames()
        {
            yield return new object[] { TestQueues.RequestProcessor };
            yield return new object[] { TestQueues.ResponseProcessor };
            yield return new object[] { TestQueues.RequestBehavior };
            yield return new object[] { TestQueues.ResponseBehavior };
        }

        [Theory(DisplayName = "Topic and subscriptions are recreated"), MemberData(nameof(QueueNames))]
        public async Task Step01(string subscriptionName)
        {
            if (await managementClient.TopicExistsAsync(topicPath) == false)
            {
                await managementClient.CreateTopicAsync(topicPath);
            }

            if (await managementClient.SubscriptionExistsAsync(topicPath, subscriptionName) == true)
            {
                await managementClient.DeleteSubscriptionAsync(topicPath, subscriptionName);
            }

            await managementClient.CreateSubscriptionAsync(topicPath, subscriptionName);
        }

        [Fact(DisplayName = "Send extensions are executed")]
        public async Task Step02()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<TopicClient>(sp => new TopicClient(connectionString, topicPath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTopicOptions<EchoRequest, EchoResponse>()
                .AddSendTopicMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Subscriptions have messages"), MemberData(nameof(QueueNames))]
        public async Task Step03(string subscriptionName)
        {
            var runtimeInfo = await managementClient.GetSubscriptionRuntimeInfoAsync(topicPath, subscriptionName);

            runtimeInfo.MessageCount.Should().Be(4);
        }

        [Theory(DisplayName = "Receive extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step04(string subscriptionName)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<SubscriptionClient>(sp => new SubscriptionClient(connectionString, topicPath, subscriptionName))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddSubscriptionOptions<EchoRequest, EchoResponse>()
                .AddReceiveSubscriptionMessageExtensions<EchoRequest, EchoResponse>(subscriptionName)

                .BuildServiceProvider();

            using var cancelSource = new CancellationTokenSource(3000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Subscriptions have messages"), MemberData(nameof(QueueNames))]
        public async Task Step05(string subscriptionName)
        {
            var runtimeInfo = await managementClient.GetSubscriptionRuntimeInfoAsync(topicPath, subscriptionName);

            runtimeInfo.MessageCount.Should().Be(0);
        }
    }
}
