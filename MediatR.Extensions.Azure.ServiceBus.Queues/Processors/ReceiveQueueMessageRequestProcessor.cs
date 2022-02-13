using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveQueueMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public ReceiveQueueMessageRequestProcessor(ReceiveQueueMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
