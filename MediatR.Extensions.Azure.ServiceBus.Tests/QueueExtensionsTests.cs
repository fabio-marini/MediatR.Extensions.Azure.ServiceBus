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
    public class TestQueues
    {
        public const string RequestProcessor = "request-processor";
        public const string ResponseProcessor = "response-processor";
        public const string RequestBehavior = "request-behavior";
        public const string ResponseBehavior = "response-behavior";
    }

    [Trait("TestCategory", "Integration")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class QueueExtensionsTests
    {
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.core.messagereceiver?view=azure-dotnet
        // TODO: add project that uses message sender and receiver + test with queues/topics and sessions

        // TODO: session command and integration test
        // TODO: schedule/cancel command (use context for enqueueTime and sequenceNumber) and integration test

        // TODO: list contoso/fabrikam examples (not integration tests)

        // TODO: to receive messages from the DLQ use /$deadletterqueue path (also see EntityNameHelper)

        // TODO: add message and exception handlers as default options? Also support MessageHandlerOptions?
        // TODO: add commands unit tests + docs
        // TODO: examples with custom message/exception handlers and receive policy

        // TODO: update storage test fixtures so tables/containers are deleted on dispose?
        // FIXME: BlobClient is a delegate, but AS table and queue clients are instances - what should SB topic and queue clients be?!?

        private readonly ITestOutputHelper log;

        private readonly string connectionString;
        private readonly ManagementClient managementClient;

        public QueueExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

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

        [Theory(DisplayName = "Queues are recreated"), MemberData(nameof(QueueNames))]
        public async Task Step01(string queuePath)
        {
            if (await managementClient.QueueExistsAsync(queuePath))
            {
                var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

                if (runtimeInfo.MessageCount > 0)
                {
                    // only recreate queue if it has any messages...
                    await managementClient.DeleteQueueAsync(queuePath);

                    await managementClient.CreateQueueAsync(queuePath);
                }
            }
            else
            {
                await managementClient.CreateQueueAsync(queuePath);
            }
        }

        [Theory(DisplayName = "Send extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step02(string queuePath)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<QueueClient>(sp => new QueueClient(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddQueueOptions<EchoRequest, EchoResponse>()
                .AddSendQueueMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Step03(string queuePath)
        {
            var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

            runtimeInfo.MessageCount.Should().Be(4);
        }

        [Theory(DisplayName = "Receive extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step04(string queuePath)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<QueueClient>(sp => new QueueClient(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddQueueOptions<EchoRequest, EchoResponse>()
                .AddReceiveQueueMessageExtensions<EchoRequest, EchoResponse>(queuePath)

                .BuildServiceProvider();

            using var cancelSource = new CancellationTokenSource(3000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Step05(string queuePath)
        {
            var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

            runtimeInfo.MessageCount.Should().Be(0);
        }
    }
}
