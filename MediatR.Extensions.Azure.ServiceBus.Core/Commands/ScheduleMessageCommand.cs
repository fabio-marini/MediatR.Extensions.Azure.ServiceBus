using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ScheduleMessageCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<MessageOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public ScheduleMessageCommand(IOptions<MessageOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
        {
            this.opt = opt;
            this.ctx = ctx;
            this.log = log ?? NullLogger.Instance;
        }

        public virtual async Task ExecuteAsync(TMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (opt.Value.IsEnabled == false)
            {
                log.LogDebug("Command {Command} is not enabled, returning", this.GetType().Name);

                return;
            }

            if (opt.Value.Sender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Sender");
            }

            if (opt.Value.Message == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Message");
            }

            var messageSender = opt.Value.Sender(message, ctx);

            if (messageSender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Sender");
            }

            var msg = opt.Value.Message(message, ctx);

            if (msg == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Message");
            }

            try
            {
                if (ctx != null && ctx.ContainsKey(ContextKeys.EnqueueTimeUtc))
                {
                    // retrieve enqueue time from context and schedule message
                    var enqueueTimeUtc = (DateTimeOffset)ctx[ContextKeys.EnqueueTimeUtc];

                    var sequenceNumber = await messageSender.ScheduleMessageAsync(msg, enqueueTimeUtc, cancellationToken);

                    if (ctx.ContainsKey(ContextKeys.SequenceNumbers) == false)
                    {
                        ctx.Add(ContextKeys.SequenceNumbers, new Queue<long>());
                    }

                    // add scheduled message sequence number to context if required for cancellation
                    var sequenceNumbers = (Queue<long>)ctx[ContextKeys.SequenceNumbers];

                    sequenceNumbers.Enqueue(sequenceNumber);
                }

                log.LogDebug("Command {Command} completed", this.GetType().Name);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }

            await messageSender.CloseAsync();
        }
    }
}
