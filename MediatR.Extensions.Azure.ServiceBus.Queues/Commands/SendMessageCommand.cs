using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus.Queues
{
    public class SendMessageCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<QueueOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public SendMessageCommand(IOptions<QueueOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
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

            if (opt.Value.QueueClient == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid QueueClient");
            }

            if (opt.Value.Message == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Message");
            }

            try
            {
                var queueClient = opt.Value.QueueClient(message, ctx);

                var queueMessage = opt.Value.Message(message, ctx);

                await queueClient.SendAsync(queueMessage);

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
