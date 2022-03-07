using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests.Core
{
    [Trait("TestCategory", "Integration"), Collection("QueueTests")]
    [TestCaseOrderer("Timeless.Testing.Xunit.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class QueueExtensionsTests
    {
        private readonly ITestOutputHelper log;
        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        public QueueExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ServiceBusAdministrationClient(connectionString));
        }

        [Fact(DisplayName = "01. Queues are recreated")]
        public async Task Step01() => await managementFixture.QueueIsRecreated(TestEntities.QueuePath);

        [Fact(DisplayName = "02. Send extensions are executed")]
        public async Task Step02()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<ServiceBusSender>(sp => new ServiceBusClient(connectionString).CreateSender(TestEntities.QueuePath))
                .AddLogging(log)
                .AddMessageOptions()
                .AddSendMessageExtensions()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);

            await serviceProvider.GetRequiredService<ServiceBusSender>().CloseAsync();
        }

        [Fact(DisplayName = "03. Queues have messages")]
        public async Task Step03() => await managementFixture.QueueHasMessages(TestEntities.QueuePath, 4);

        [Fact(DisplayName = "04. Receive extensions are executed")]
        public async Task Step04()
        {
            var receiveOptions = new ServiceBusReceiverOptions { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete };

            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<ServiceBusReceiver>(sp => new ServiceBusClient(connectionString).CreateReceiver(TestEntities.QueuePath, receiveOptions))
                .AddLogging(log)
                .AddMessageOptions()
                .AddReceiveMessageExtensions()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);

            await serviceProvider.GetRequiredService<ServiceBusReceiver>().CloseAsync();
        }

        [Fact(DisplayName = "05. Queues have messages")]
        public async Task Step05() => await managementFixture.QueueHasMessages(TestEntities.QueuePath, 0);
    }
}
