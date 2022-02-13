using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus.Queues
{
    public class RegisterMessageHandlerRequestBehavior<TRequest> : RegisterMessageHandlerRequestBehavior<TRequest, Unit> where TRequest : IRequest<Unit>
    {
        public RegisterMessageHandlerRequestBehavior(RegisterMessageHandlerCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }

    public class RegisterMessageHandlerRequestBehavior<TRequest, TResponse> : RequestBehaviorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public RegisterMessageHandlerRequestBehavior(RegisterMessageHandlerCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }
}
