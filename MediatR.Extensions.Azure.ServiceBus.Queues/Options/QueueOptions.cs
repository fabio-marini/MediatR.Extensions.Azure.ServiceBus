using MediatR.Extensions.Abstractions;
using Microsoft.Azure.ServiceBus;
using System;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus.Queues
{
    public class QueueOptions<TMessage>
    {
        public virtual bool IsEnabled { get; set; }

        public virtual Func<TMessage, PipelineContext, QueueClient> QueueClient { get; set; }
        public virtual Func<TMessage, PipelineContext, Message> Message { get; set; }

        // the event that is raised after the queue message is received (allows using the message to modify TMessage)
        public virtual Func<Message, PipelineContext, TMessage, Task> Received { get; set; }
    }
}
