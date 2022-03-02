using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveMessageCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<MessageOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public ReceiveMessageCommand(IOptions<MessageOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
        {
            this.opt = opt;
            this.ctx = ctx;
            this.log = log ?? NullLogger.Instance;
        }

        public virtual async Task ExecuteAsync(TMessage msg, CancellationToken tkn)
        {
            tkn.ThrowIfCancellationRequested();

            if (opt.Value.IsEnabled == false)
            {
                log.LogDebug("Command {Command} is not enabled, returning", this.GetType().Name);

                return;
            }

            var messageReceiver = opt.Value.Receiver?.Invoke(msg, ctx);

            if (messageReceiver == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Receiver");
            }
            
            try
            {
                var targetMessage = await messageReceiver.ReceiveMessageAsync(cancellationToken: tkn);

                if (opt.Value.Received != null)
                {
                    await opt.Value.Received(targetMessage, ctx, msg);
                }

                log.LogDebug("Command {Command} completed", this.GetType().Name);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }
            finally
            {
                await messageReceiver.CloseAsync(tkn);
            }
        }
    }
}
