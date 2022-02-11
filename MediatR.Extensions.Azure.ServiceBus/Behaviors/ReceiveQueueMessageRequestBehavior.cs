using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveQueueMessageRequestBehavior<TRequest> : ReceiveQueueMessageRequestBehavior<TRequest, Unit> where TRequest : IRequest<Unit>
    {
        public ReceiveQueueMessageRequestBehavior(ReceiveQueueMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }

    public class ReceiveQueueMessageRequestBehavior<TRequest, TResponse> : RequestBehaviorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public ReceiveQueueMessageRequestBehavior(ReceiveQueueMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }
}
