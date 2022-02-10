using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class IntegrationTests
    {
        private readonly ITestOutputHelper log;

        public IntegrationTests(ITestOutputHelper log)
        {
            this.log = log;

            // TODO: create appropriate queue/topic fixtures
            // FIXME: BlobClient is a delegate, but table and queue clients are instances - what should topic and queue client be?!?
        }

        [Fact]
        public async Task Test01()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<QueueClient>(sp =>
                {
                    var connectionString = "";

                    return new QueueClient(connectionString, "mediator-simple");
                })
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddQueueOptions<EchoRequest, EchoResponse>()
                .AddSendQueueMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact]
        public async Task Test02()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<TopicClient>(sp =>
                {
                    var connectionString = "";

                    return new TopicClient(connectionString, "mediator-topic");
                })
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTopicOptions<EchoRequest, EchoResponse>()
                .AddSendTopicMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }
    }
}
