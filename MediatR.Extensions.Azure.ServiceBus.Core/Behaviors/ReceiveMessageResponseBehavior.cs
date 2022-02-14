using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveMessageResponseBehavior<TRequest, TResponse> : ResponseBehaviorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public ReceiveMessageResponseBehavior(ReceiveMessageCommand<TResponse> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
