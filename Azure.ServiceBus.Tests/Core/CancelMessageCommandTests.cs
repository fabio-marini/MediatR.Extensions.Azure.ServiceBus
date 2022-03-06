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
    public class CancelMessageCommandTests
    {
        private readonly IServiceProvider svc;
        private readonly Mock<MessageOptions<EchoRequest>> opt;
        private readonly Mock<ServiceBusSender> snd;

        private readonly CancelMessageCommand<EchoRequest> cmd;

        public CancelMessageCommandTests()
        {
            opt = new Mock<MessageOptions<EchoRequest>>();
            snd = new Mock<ServiceBusSender>();

            svc = new ServiceCollection()

                .AddTransient<CancelMessageCommand<EchoRequest>>()
                .AddTransient<IOptions<MessageOptions<EchoRequest>>>(sp => Options.Create(opt.Object))

                .BuildServiceProvider();

            cmd = svc.GetRequiredService<CancelMessageCommand<EchoRequest>>();
        }

        [Fact(DisplayName = "Command is disabled")]
        public async Task Test1()
        {
            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Never);
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

        [Fact(DisplayName = "Sender is not specified")]
        public async Task Test3()
        {
            opt.SetupProperty(m => m.IsEnabled, true);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Once);
            opt.VerifyGet(m => m.Message, Times.Never);
        }

        [Fact(DisplayName = "Sequence number has no value")]
        public async Task Test4()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.SequenceNumber, null);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Once);
            opt.VerifyGet(m => m.SequenceNumber, Times.Once);

            opt.Verify(m => m.Sender.CancelScheduledMessageAsync(It.IsAny<long>(), CancellationToken.None), Times.Never);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Never);
        }

        [Fact(DisplayName = "Sequence number has value")]
        public async Task Test5()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.SequenceNumber, (ctx, req) => 1L);

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(2));
            opt.VerifyGet(m => m.SequenceNumber, Times.Once);

            opt.Verify(m => m.Sender.CancelScheduledMessageAsync(It.IsAny<long>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Never);
        }

        [Fact(DisplayName = "Command throws CommandException")]
        public async Task Test6()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.SequenceNumber, (ctx, req) => 1L);

            snd.Setup(m => m.CancelScheduledMessageAsync(It.IsAny<long>(), CancellationToken.None)).ThrowsAsync(new ArgumentNullException());

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<CommandException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(2));
            opt.VerifyGet(m => m.SequenceNumber, Times.Exactly(1));

            opt.Verify(m => m.Sender.CancelScheduledMessageAsync(It.IsAny<long>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Never);
        }
    }
}
