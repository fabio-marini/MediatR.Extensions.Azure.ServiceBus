using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveSubscriptionMessageRequestBehavior<TRequest> : ReceiveSubscriptionMessageRequestBehavior<TRequest, Unit> where TRequest : IRequest<Unit>
    {
        public ReceiveSubscriptionMessageRequestBehavior(ReceiveSubscriptionMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }

    public class ReceiveSubscriptionMessageRequestBehavior<TRequest, TResponse> : RequestBehaviorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public ReceiveSubscriptionMessageRequestBehavior(ReceiveSubscriptionMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }
}
