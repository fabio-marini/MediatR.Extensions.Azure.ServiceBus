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

        public virtual async Task ExecuteAsync(TMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (opt.Value.IsEnabled == false)
            {
                log.LogDebug("Command {Command} is not enabled, returning", this.GetType().Name);
            }

            if (opt.Value.MessageSender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Receiver");
            }

            if (opt.Value.Message == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Message");
            }

            var messageSender = opt.Value.MessageSender(message, ctx);

            try
            {
                await messageSender.SendAsync(opt.Value.Message(message, ctx));

                await messageSender.CloseAsync();

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
