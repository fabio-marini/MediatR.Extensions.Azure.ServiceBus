using MediatR.Extensions.Abstractions;
using Microsoft.Azure.ServiceBus;
using System;

namespace MediatR.Extensions.Azure.ServiceBus.Topics
{
    public class TopicOptions<TMessage>
    {
        public virtual bool IsEnabled { get; set; }

        public virtual Func<TMessage, PipelineContext, TopicClient> TopicClient { get; set; }
        public virtual Func<TMessage, PipelineContext, Message> Message { get; set; }
    }
}
