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
    public class ScheduleMessageCommandTests
    {
        private readonly IServiceProvider svc;
        private readonly Mock<MessageOptions<EchoRequest>> opt;
        private readonly Mock<ServiceBusSender> snd;

        private readonly ScheduleMessageCommand<EchoRequest> cmd;

        public ScheduleMessageCommandTests()
        {
            opt = new Mock<MessageOptions<EchoRequest>>();
            snd = new Mock<ServiceBusSender>();

            svc = new ServiceCollection()

                .AddTransient<ScheduleMessageCommand<EchoRequest>>()
                .AddTransient<IOptions<MessageOptions<EchoRequest>>>(sp => Options.Create(opt.Object))

                .BuildServiceProvider();

            cmd = svc.GetRequiredService<ScheduleMessageCommand<EchoRequest>>();
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

        [Fact(DisplayName = "Enqueue time is not specified")]
        public async Task Test4()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Once);
            opt.VerifyGet(m => m.Message, Times.Never);
        }

        [Fact(DisplayName = "Enqueue time is in the past")]
        public async Task Test5()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.EnqueueTime, (req, ctx) => default);

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Once);
            opt.VerifyGet(m => m.Message, Times.Never);
        }

        [Fact(DisplayName = "Command uses default Message")]
        public async Task Test6()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.Message, null);
            opt.SetupProperty(m => m.EnqueueTime, (req, ctx) => DateTimeOffset.UtcNow.AddMinutes(1));

            snd.Setup(m => m.ScheduleMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<DateTimeOffset>(), CancellationToken.None)).Returns(Task.FromResult(1L));

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(2));
            opt.VerifyGet(m => m.Message, Times.Exactly(1));
            opt.VerifyGet(m => m.Scheduled, Times.Once);

            var capturedMessages = new List<ServiceBusMessage>();

            opt.Verify(m => m.Sender.ScheduleMessageAsync(Capture.In(capturedMessages), It.IsAny<DateTimeOffset>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Never);

            var echoRequest = JsonConvert.DeserializeObject<EchoRequest>(capturedMessages.Single().Body.ToString());

            echoRequest.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact(DisplayName = "Command uses specified Message")]
        public async Task Test7()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.Message, (cmd, ctx) => new ServiceBusMessage(BinaryData.FromString("Hello world"))
            {
                ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddMinutes(10)
            });
            opt.SetupProperty(m => m.EnqueueTime, (req, ctx) => DateTimeOffset.UtcNow.AddMinutes(1));

            snd.Setup(m => m.ScheduleMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<DateTimeOffset>(), CancellationToken.None)).Returns(Task.FromResult(1L));

            await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(2));
            opt.VerifyGet(m => m.Message, Times.Exactly(1));
            opt.VerifyGet(m => m.Scheduled, Times.Once);

            var capturedMessages = new List<ServiceBusMessage>();

            opt.Verify(m => m.Sender.ScheduleMessageAsync(Capture.In(capturedMessages), It.IsAny<DateTimeOffset>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Never);

            capturedMessages.Single().ScheduledEnqueueTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(10));

            capturedMessages.Single().Body.ToString().Should().Be("Hello world");
        }

        [Fact(DisplayName = "Command throws CommandException")]
        public async Task Test8()
        {
            opt.SetupProperty(m => m.IsEnabled, true);
            opt.SetupProperty(m => m.Sender, snd.Object);
            opt.SetupProperty(m => m.Message, null);
            opt.SetupProperty(m => m.EnqueueTime, (req, ctx) => DateTimeOffset.UtcNow.AddMinutes(1));

            snd.Setup(m => m.ScheduleMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<DateTimeOffset>(), CancellationToken.None)).ThrowsAsync(new ArgumentNullException());

            Func<Task> act = async () => await cmd.ExecuteAsync(EchoRequest.Default, CancellationToken.None);

            await act.Should().ThrowAsync<CommandException>();

            opt.VerifyGet(m => m.IsEnabled, Times.Once);
            opt.VerifyGet(m => m.Sender, Times.Exactly(2));
            opt.VerifyGet(m => m.Message, Times.Exactly(1));

            opt.Verify(m => m.Sender.ScheduleMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<DateTimeOffset>(), CancellationToken.None), Times.Once);
            opt.Verify(m => m.Sender.CloseAsync(CancellationToken.None), Times.Never);
        }
    }
}
