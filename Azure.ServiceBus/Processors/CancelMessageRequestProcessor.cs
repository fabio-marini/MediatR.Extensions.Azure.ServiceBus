using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class CancelMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public CancelMessageRequestProcessor(CancelMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
