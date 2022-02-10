using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class SendQueueMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public SendQueueMessageRequestProcessor(SendQueueMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
