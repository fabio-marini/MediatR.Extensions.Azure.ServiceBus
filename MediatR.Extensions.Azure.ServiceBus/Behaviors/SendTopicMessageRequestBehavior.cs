using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class SendTopicMessageRequestBehavior<TRequest> : SendTopicMessageRequestBehavior<TRequest, Unit> where TRequest : IRequest<Unit>
    {
        public SendTopicMessageRequestBehavior(SendTopicMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }

    public class SendTopicMessageRequestBehavior<TRequest, TResponse> : RequestBehaviorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public SendTopicMessageRequestBehavior(SendTopicMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null)
            : base(cmd, ctx, log)
        {
        }
    }
}
