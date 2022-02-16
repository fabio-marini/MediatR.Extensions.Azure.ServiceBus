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
    public class CancelMessageCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<MessageOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public CancelMessageCommand(IOptions<MessageOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
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

            if (opt.Value.MessageSender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Sender");
            }

            var messageSender = opt.Value.MessageSender(message, ctx);

            if (messageSender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Sender");
            }

            if (ctx == null || ctx.ContainsKey(ContextKeys.SequenceNumbers) == false)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Context");
            }

            try
            {
                var sequenceNumbers = (Queue<long>)ctx[ContextKeys.SequenceNumbers];

                var sequenceNumber = sequenceNumbers.Dequeue();

                log.LogInformation("Cancelling message {Id}", sequenceNumber);

                await messageSender.CancelScheduledMessageAsync(sequenceNumber);

                log.LogDebug("Command {Command} completed", this.GetType().Name);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }
        }
    }
}
