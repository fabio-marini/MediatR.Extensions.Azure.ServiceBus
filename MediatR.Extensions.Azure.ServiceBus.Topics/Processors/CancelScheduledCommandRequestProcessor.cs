using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus.Topics
{
    public class CancelScheduledCommandRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public CancelScheduledCommandRequestProcessor(CancelScheduledCommandCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
