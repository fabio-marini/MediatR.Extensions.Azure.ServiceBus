using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus.Topics
{
    public class RegisterSessionHandlerRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public RegisterSessionHandlerRequestProcessor(RegisterSessionHandlerCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
