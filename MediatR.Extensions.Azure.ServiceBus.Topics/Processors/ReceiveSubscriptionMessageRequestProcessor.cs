using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class ReceiveSubscriptionMessageRequestProcessor<TRequest> : RequestProcessorBase<TRequest>
    {
        public ReceiveSubscriptionMessageRequestProcessor(ReceiveSubscriptionMessageCommand<TRequest> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}
