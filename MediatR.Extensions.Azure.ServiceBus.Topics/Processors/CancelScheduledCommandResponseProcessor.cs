﻿using MediatR.Extensions.Abstractions;
using Microsoft.Extensions.Logging;

namespace MediatR.Extensions.Azure.ServiceBus.Topics
{
    public class CancelScheduledCommandResponseProcessor<TRequest, TResponse> : ResponseProcessorBase<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public CancelScheduledCommandResponseProcessor(CancelScheduledCommandCommand<TResponse> cmd, PipelineContext ctx = null, ILogger log = null) : base(cmd, ctx, log)
        {
        }
    }
}