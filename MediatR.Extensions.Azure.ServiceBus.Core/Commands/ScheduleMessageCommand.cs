using Azure.Messaging.ServiceBus;
using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
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

        public virtual async Task ExecuteAsync(TMessage msg, CancellationToken tkn)
        {
            tkn.ThrowIfCancellationRequested();

            if (opt.Value.IsEnabled == false)
            {
                log.LogDebug("Command {Command} is not enabled, returning", this.GetType().Name);

                return;
            }

            if (opt.Value.Sender == null)
            {
                throw new ArgumentNullException($"Command {this.GetType().Name} requires a valid Sender");
            }

            var targetMessage = opt.Value.Message?.Invoke(msg, ctx) ?? new ServiceBusMessage(JsonConvert.SerializeObject(msg));

            try
            {
                // retrieve enqueue time from context or default it 
                var enqueueTime = (ctx == null || !ctx.ContainsKey(ContextKeys.EnqueueTimeUtc))
                    ? DateTimeOffset.UtcNow
                    : (DateTimeOffset)ctx[ContextKeys.EnqueueTimeUtc];

                var sequenceNumber = await opt.Value.Sender.ScheduleMessageAsync(targetMessage, enqueueTime, tkn);

                log.LogDebug("Command {Command} scheduled message {SequenceNumber}", this.GetType().Name, sequenceNumber);

                await opt.Value.Scheduled?.Invoke(sequenceNumber, targetMessage, ctx, msg);
            }
            catch (Exception ex)
            {
                log.LogDebug(ex, "Command {Command} failed with message: {Message}", this.GetType().Name, ex.Message);

                throw new CommandException($"Command {this.GetType().Name} failed, see inner exception for details", ex);
            }
            finally
            {
                await opt.Value.Sender.CloseAsync(tkn);
            }
        }
    }
}
