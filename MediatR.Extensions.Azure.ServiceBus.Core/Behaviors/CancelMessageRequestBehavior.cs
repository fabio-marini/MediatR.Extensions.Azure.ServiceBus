using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class CancelMessageRequestBehavior<TRequest> : RequestBehaviorBase<TRequest, Unit> where TRequest : IRequest<Unit>
    {
        public CancelMessageRequestBehavior(CancelMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }

    public class CancelMessageRequestBehavior<TRequest, TResponse> : RequestBehaviorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public CancelMessageRequestBehavior(CancelMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }
}
