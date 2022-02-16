using MediatR.Extensions.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus.Queues
{
    public class RegisterMessageHandlerCommand<TMessage> : ICommand<TMessage>
    {
        private readonly IOptions<QueueOptions<TMessage>> opt;
        private readonly PipelineContext ctx;
        private readonly ILogger log;

        public RegisterMessageHandlerCommand(IOptions<QueueOptions<TMessage>> opt, PipelineContext ctx = null, ILogger log = null)
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

            var messageHandler = new Func<Message, CancellationToken, Task>(async (msg, tkn) =>
            {
                log.LogDebug($"{DateTime.Now.ToString("hh:mm:ss.fff")} - Received message {msg.MessageId}");

                if (opt.Value.Received != null)
                {
                    await opt.Value.Received(msg, ctx, message);
                }
            });

            var exceptionHandler = new Func<ExceptionReceivedEventArgs, Task>((args) =>
            {
                log.LogDebug($"{DateTime.Now.ToString("hh:mm:ss.fff")} - Received exception {args.Exception.Message}");

                return Task.CompletedTask;
            });

            var queueClient = opt.Value.QueueClient(message, ctx);

            try
            {
                queueClient.RegisterMessageHandler(messageHandler, exceptionHandler);

                var receivePolicy = Policy
                    .HandleResult<CancellationToken>(tkn => tkn.IsCancellationRequested == false)
                    .WaitAndRetryForever(i => TimeSpan.FromMilliseconds(500));

                receivePolicy.Execute(() =>
                {
                    log.LogDebug($"{DateTime.Now.ToString("hh:mm:ss.fff")} - Executing receive policy");

                    return cancellationToken;
                });

                log.LogDebug("Command {Command} completed", this.GetType().Name);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }
            finally
            {
                await queueClient.UnregisterMessageHandlerAsync(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
