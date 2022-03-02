using Azure;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class ReceiveMessageCommandTests
    {
        private readonly IServiceProvider svc;
        private readonly Mock<MessageOptions<EchoRequest>> opt;
        private readonly Mock<ServiceBusReceiver> rcv;

        private readonly ReceiveMessageCommand<EchoRequest> cmd;

        public ReceiveMessageCommandTests()
        {
            opt = new Mock<MessageOptions<EchoRequest>>();
            rcv = new Mock<ServiceBusReceiver>();

            svc = new ServiceCollection()

                .AddTransient<ReceiveMessageCommand<EchoRequest>>()
                .AddTransient<IOptions<MessageOptions<EchoRequest>>>(sp => Options.Create(opt.Object))

                .BuildServiceProvider();

            cmd = svc.GetRequiredService<ReceiveMessageCommand<EchoRequest>>();
        }

        [Fact(DisplayName = "Command is disabled")]
        public async Task Test1()
        {
            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Receiver, Times.Never);
            opt.VerifyGet(m => m.Message, Times.Never);
        }

        [Fact(DisplayName = "Command is cancelled")]
        public async Task Test2()
        {
            var src = new CancellationTokenSource(0);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, src.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Never);
        }

        [Fact(DisplayName = "Receiver is not specified")]
        public async Task Test3()
        {
            opt.SetupProperty(m => m.IsEnabled, true);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Receiver, Times.Once);
            opt.VerifyGet(m => m.Message, Times.Never);
        }

        [Fact(DisplayName = "Command throws CommandException")]
        public async Task Test4()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Receiver, rcv.Object);

            rcv.Setup(m => m.ReceiveMessageAsync(It.IsAny<TimeSpan?>(), CancellationToken.None))
                .ThrowsAsync(new ArgumentNullException());

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<CommandException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Receiver, Times.Exactly(3));

            opt.Verify(m => m.Receiver.ReceiveMessageAsync(It.IsAny<TimeSpan?>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Receiver.CloseAsync(CancellationToken.None), Times.Once);
        }

        [Fact(DisplayName = "Command completes successfully")]
        public async Task Test5()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Receiver, rcv.Object);

            var res = new Mock<Response>();
            res.SetupGet(r => r.Status).Returns(200);

            rcv.Setup(m => m.ReceiveMessageAsync(It.IsAny<TimeSpan?>(), CancellationToken.None))
                .ReturnsAsync(Response.FromValue<ServiceBusReceivedMessage>(default, res.Object));

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Receiver, Times.Exactly(3));
            opt.VerifyGet(m => m.Received, Times.Once);

            opt.Verify(m => m.Receiver.ReceiveMessageAsync(It.IsAny<TimeSpan?>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Receiver.CloseAsync(CancellationToken.None), Times.Once);
        }
    }
}
