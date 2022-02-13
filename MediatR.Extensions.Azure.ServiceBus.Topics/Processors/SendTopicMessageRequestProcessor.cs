using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class SendTopicMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public SendTopicMessageRequestProcessor(SendTopicMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
