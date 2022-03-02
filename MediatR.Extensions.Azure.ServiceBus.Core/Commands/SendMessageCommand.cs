using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class SendMessageCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<MessageOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public SendMessageCommand(IOptions<MessageOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
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

            var messageSender = opt.Value.Sender?.Invoke(msg, ctx);

            if (messageSender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Sender");
            }

            var targetMessage = opt.Value.Message?.Invoke(msg, ctx);

            if (targetMessage == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Message");
            }

            try
            {
                await messageSender.SendMessageAsync(targetMessage, tkn);

                log.LogDebug("Command {Command} completed", this.GetType().Name);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }
            finally
            {
                await messageSender.CloseAsync(tkn);
            }
        }
    }
}
