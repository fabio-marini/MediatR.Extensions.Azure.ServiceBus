using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using FluentAssertions;
using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Extensions.Azure.Storage.Tests.Commands.Queues
{
    public class SendMessageCommandTests
    {
        private readonly IServiceProvider svc;
        private readonly Mock<QueueOptions<EchoRequest>> opt;
        private readonly Mock<QueueClient> que;

        private readonly SendMessageCommand<EchoRequest> cmd;

        public SendMessageCommandTests()
        {
            opt = new Mock<QueueOptions<EchoRequest>>();
            que = new Mock<QueueClient>("UseDevelopmentStorage=true", "queue1");

            svc = new ServiceCollection()

                .AddTransient<SendMessageCommand<EchoRequest>>()
                .AddTransient<IOptions<QueueOptions<EchoRequest>>>(sp => Options.Create(opt.Object))

                .BuildServiceProvider();

            cmd = svc.GetRequiredService<SendMessageCommand<EchoRequest>>();
        }

        [Fact(DisplayName = "Command is disabled")]
        public async Task Test1()
        {
            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.QueueClient, Times.Never);
            opt.VerifyGet(m => m.QueueMessage, Times.Never);
        }

        [Fact(DisplayName = "Command is cancelled")]
        public async Task Test2()
        {
            var src = new CancellationTokenSource(0);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, src.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Never);
        }

        [Fact(DisplayName = "QueueClient is not specified")]
        public async Task Test3()
        {
            opt.SetupProperty(m => m.IsEnabled, true);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.QueueClient, Times.Once);
            opt.VerifyGet(m => m.QueueMessage, Times.Never);
        }

        [Fact(DisplayName = "Command uses default QueueMessage")]
        public async Task Test4()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.QueueClient, que.Object);
            opt.SetupProperty(m => m.QueueMessage, null);

            var res = new Mock<Response>();
            res.SetupGet(r => r.Status).Returns(200);

            que.Setup(m => m.SendMessageAsync(It.IsAny<BinaryData>(), opt.Object.Visibility, opt.Object.TimeToLive, CancellationToken.None))
                .ReturnsAsync(Response.FromValue<SendReceipt>(default, res.Object));

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.QueueClient, Times.Exactly(2));
            opt.VerifyGet(m => m.QueueMessage, Times.Exactly(2));

            opt.VerifySet(m => m.QueueMessage = It.IsAny<Func<EchoRequest, PipelineContext, BinaryData>>(), Times.Once);

            opt.Verify(m => m.QueueClient.SendMessageAsync(It.IsAny<BinaryData>(), opt.Object.Visibility, opt.Object.TimeToLive, CancellationToken.None), Times.Once);
        }

        [Fact(DisplayName = "Command uses specified QueueMessage")]
        public async Task Test5()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.QueueClient, que.Object);
            opt.SetupProperty(m => m.QueueMessage, (cmd, ctx) => BinaryData.FromString("Hello world"));

            var res = new Mock<Response>();
            res.SetupGet(r => r.Status).Returns(200);

            que.Setup(m => m.SendMessageAsync(It.IsAny<BinaryData>(), opt.Object.Visibility, opt.Object.TimeToLive, CancellationToken.None))
                .ReturnsAsync(Response.FromValue<SendReceipt>(default, res.Object));

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.QueueClient, Times.Exactly(2));
            opt.VerifyGet(m => m.QueueMessage, Times.Exactly(2));

            opt.VerifySet(m => m.QueueMessage = It.IsAny<Func<EchoRequest, PipelineContext, BinaryData>>(), Times.Never);

            opt.Verify(m => m.QueueClient.SendMessageAsync(It.IsAny<BinaryData>(), opt.Object.Visibility, opt.Object.TimeToLive, CancellationToken.None), Times.Once);
        }

        [Fact(DisplayName = "Command throws CommandException")]
        public async Task Test6()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.QueueClient, que.Object);
            opt.SetupProperty(m => m.QueueMessage, (cmd, ctx) => BinaryData.FromString("Hello world"));

            que.Setup(m => m.SendMessageAsync(It.IsAny<BinaryData>(), opt.Object.Visibility, opt.Object.TimeToLive, CancellationToken.None))
                .ThrowsAsync(new ArgumentNullException());

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<CommandException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.QueueClient, Times.Exactly(2));
            opt.VerifyGet(m => m.QueueMessage, Times.Exactly(2));

            opt.VerifySet(m => m.QueueMessage = It.IsAny<Func<EchoRequest, PipelineContext, BinaryData>>(), Times.Never);

            opt.Verify(m => m.QueueClient.SendMessageAsync(It.IsAny<BinaryData>(), opt.Object.Visibility, opt.Object.TimeToLive, CancellationToken.None), Times.Once);
        }
    }
}
