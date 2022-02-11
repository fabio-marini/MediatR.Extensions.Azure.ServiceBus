using FluentAssertions;
using MediatR.Pipeline;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class TestMethodNameOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            return testCases.OrderBy(t => t.TestMethod.Method.Name);
        }
    }

    [Trait("TestCategory", "Integration")]
    [TestCaseOrderer("MediatR.Extensions.Azure.ServiceBus.Tests.TestMethodNameOrderer", "MediatR.Extensions.Azure.ServiceBus.Tests")]
    public class QueueExtensionsTests
    {
        // FIXME: IRequestPreProcessor has received messages from all queues because of options config?!?
        // FIXME: avoid having multiple extensions receiving on the same queue?
        // FIXME: restore AddExtensions for receive (only has preproc)
        // TODO: add TestOutputLogger + cancel if queue has zero messages

        // FIXME: TT Xunit extension breaks IConfig (gives GetValue doesn't exist)! :(

        // TODO: delete management fixture if not used...

        // TODO: share message and exception handlers in a single ManagementFixture?
        // TODO: write integration tests without fixtures first, then add if necessary (management client has all features)...

        // TODO: add a receive policy that stops after receiving all messages
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
            yield return new object[] { "mediator/request-processor" };
            yield return new object[] { "mediator/response-processor" };
            yield return new object[] { "mediator/request-behavior" };
            yield return new object[] { "mediator/response-behavior" };
        }

        [Theory(DisplayName = "Queues are recreated"), MemberData(nameof(QueueNames))]
        public async Task Test01(string queuePath)
        {
            if (await managementClient.QueueExistsAsync(queuePath))
            {
                var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

                if (runtimeInfo.MessageCount > 0)
                {
                    // only recreate queue if it has any messsages...
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
        public async Task Test02(string queuePath)
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
        public async Task Test03(string queuePath)
        {
            var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

            runtimeInfo.MessageCount.Should().Be(4);
        }

        [Theory(DisplayName = "Receive extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Test04(string queuePath)
        {
            // FIXME: how to ensure each extension only receives the messages from its assigned queue?
            //        i.e. this should leave 0 messages in request-processor and 4 in the others
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddSingleton<QueueClient>(sp => new QueueClient(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddQueueOptions<EchoRequest, EchoResponse>()
                .AddTransient<IRequestPreProcessor<EchoRequest>, ReceiveQueueMessageRequestProcessor<EchoRequest>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<EchoRequest>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<ReceiveQueueMessageCommand<EchoRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<ReceiveQueueMessageRequestProcessor<EchoRequest>>(sp, cmd);
                })

                .BuildServiceProvider();


            // command implementation - token is received from mediator
            using var cancelSource = new CancellationTokenSource(10000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Test05(string queuePath)
        {
            var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

            runtimeInfo.MessageCount.Should().Be(0);
        }
    }
}
