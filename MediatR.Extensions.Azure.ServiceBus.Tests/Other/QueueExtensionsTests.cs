using FluentAssertions;
using MediatR.Extensions.Azure.Storage.Examples;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    [Trait("TestCategory", "Integration"), Collection("QueueTests")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class QueueExtensionsTests
    {
        // TODO: can refactor DI extensions to be non-generic if always using test echo req/res?
        // TODO: ensure private IConfig member is removed from all tests and is only used in the ctor to build the mgmt fixture...
        // TODO: can refactor all theories to be facts and use a single entity? 
        // TODO: repeat cancel tests for topics?

        // TODO: implement session
        // TODO: commands unit tests + docs

        // TODO: list contoso/fabrikam examples (not integration tests, see EntityNameHelper for DLQ)

        // TODO: update storage test fixtures so tables/containers are deleted on dispose?
        // FIXME: BlobClient is a delegate, but AS table and queue clients are instances - what should SB topic and queue clients be?!?
        // TODO: confirm core doesn't support: sessions, manual complete

        private readonly ITestOutputHelper log;

        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        public QueueExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ManagementClient(connectionString));
        }

        public static IEnumerable<object[]> QueueNames()
        {
            yield return new object[] { TestEntities.RequestProcessor };
            yield return new object[] { TestEntities.ResponseProcessor };
            yield return new object[] { TestEntities.RequestBehavior };
            yield return new object[] { TestEntities.ResponseBehavior };
        }

        [Theory(DisplayName = "Queues are recreated"), MemberData(nameof(QueueNames))]
        public async Task Step01(string queuePath) => await managementFixture.QueueIsRecreated(queuePath);

        [Theory(DisplayName = "Send extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step02(string queuePath)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<QueueClient>(sp => new QueueClient(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions
                {
                    MinimumLogLevel = LogLevel.Debug
                })
                .AddQueueOptions<EchoRequest, EchoResponse>()
                .AddSendQueueMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Step03(string queuePath) => await managementFixture.QueueHasMessages(queuePath, 4);

        [Theory(DisplayName = "Receive extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step04(string queuePath)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<QueueClient>(sp => new QueueClient(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions
                {
                    MinimumLogLevel = LogLevel.Debug
                })
                .AddQueueOptions<EchoRequest, EchoResponse>()
                .AddReceiveQueueMessageExtensions<EchoRequest, EchoResponse>(queuePath)

                .BuildServiceProvider();

            using var cancelSource = new CancellationTokenSource(3000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Step05(string queuePath) => await managementFixture.QueueHasMessages(queuePath, 0);
    }
}
