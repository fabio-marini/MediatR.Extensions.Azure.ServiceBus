using Azure;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class SendMessageCommandTests
    {
        private readonly IServiceProvider svc;
        private readonly Mock<MessageOptions<EchoRequest>> opt;
        private readonly Mock<ServiceBusSender> snd;

        private readonly SendMessageCommand<EchoRequest> cmd;

        public SendMessageCommandTests()
        {
            opt = new Mock<MessageOptions<EchoRequest>>();
            snd = new Mock<ServiceBusSender>();

            svc = new ServiceCollection()

                .AddTransient<SendMessageCommand<EchoRequest>>()
                .AddTransient<IOptions<MessageOptions<EchoRequest>>>(sp => Options.Create(opt.Object))

                .BuildServiceProvider();

            cmd = svc.GetRequiredService<SendMessageCommand<EchoRequest>>();
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

        [Fact(DisplayName = "Command uses default Message")]
        public async Task Test4()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.Message, null);

            snd.Setup(m => m.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None)).Returns(Task.CompletedTask);

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(3));
            opt.VerifyGet(m => m.Message, Times.Exactly(1));

            var capturedMessages = new List<ServiceBusMessage>();

            opt.Verify(m => m.Sender.SendMessageAsync(Capture.In(capturedMessages), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Once);

            var echoRequest = JsonConvert.DeserializeObject<EchoRequest>(capturedMessages.Single().Body.ToString());
            
            echoRequest.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact(DisplayName = "Command uses specified Message")]
        public async Task Test5()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.Message, (cmd, ctx) => new ServiceBusMessage(BinaryData.FromString("Hello world")));

            snd.Setup(m => m.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None)).Returns(Task.CompletedTask);

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(3));
            opt.VerifyGet(m => m.Message, Times.Exactly(1));

            var capturedMessages = new List<ServiceBusMessage>();

            opt.Verify(m => m.Sender.SendMessageAsync(Capture.In(capturedMessages), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Once);

            capturedMessages.Single().Body.ToString().Should().Be("Hello world");
        }

        [Fact(DisplayName = "Command throws CommandException")]
        public async Task Test6()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.Message, (cmd, ctx) => new ServiceBusMessage(BinaryData.FromString("Hello world")));

            snd.Setup(m => m.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None)).ThrowsAsync(new ArgumentNullException());

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<CommandException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(3));
            opt.VerifyGet(m => m.Message, Times.Exactly(1));

            opt.Verify(m => m.Sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Once);
        }
    }
}
