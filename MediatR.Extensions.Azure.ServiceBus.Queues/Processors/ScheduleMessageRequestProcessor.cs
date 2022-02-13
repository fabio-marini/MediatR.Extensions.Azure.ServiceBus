using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus.Queues
{
    public class ScheduleMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public ScheduleMessageRequestProcessor(ScheduleMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
