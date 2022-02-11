using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveSubscriptionMessageCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<SubscriptionOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public ReceiveSubscriptionMessageCommand(IOptions<SubscriptionOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
        {
            this.opt = opt;
            this.ctx = ctx;
            this.log = log ?? NullLogger.Instance;
        }

        public virtual Task ExecuteAsync(TMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (opt.Value.IsEnabled == false)
            {
                log.LogDebug("Command {Command} is not enabled, returning", this.GetType().Name);

                return Task.CompletedTask;
            }

            try
            {
                var receivePolicy = Policy
                    .HandleResult<CancellationToken>(tkn => tkn.IsCancellationRequested == false)
                    .WaitAndRetryForever(i => TimeSpan.FromMilliseconds(500));

                receivePolicy.Execute(() =>
                {
                    log.LogDebug($"{DateTime.Now} - Executing receive policy");

                    return cancellationToken;
                });

                log.LogDebug("Command {Command} completed", this.GetType().Name);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }

            return Task.CompletedTask;
        }
    }
}
