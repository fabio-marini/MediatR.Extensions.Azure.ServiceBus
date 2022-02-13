using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus.Topics
{
    public class RegisterMessageHandlerRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public RegisterMessageHandlerRequestProcessor(RegisterMessageHandlerCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
