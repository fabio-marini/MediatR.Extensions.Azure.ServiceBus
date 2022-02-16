using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class CancelMessageResponseProcessor<TRequest, TResponse> : ResponseProcessorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public CancelMessageResponseProcessor(CancelMessageCommand<TResponse> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
