using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public ReceiveMessageRequestProcessor(ReceiveMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
