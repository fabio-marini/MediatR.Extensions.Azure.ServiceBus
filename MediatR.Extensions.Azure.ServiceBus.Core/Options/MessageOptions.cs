using MediatR.Extensions.Abstractions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class MessageOptions<TMessage>
    {
        public virtual bool IsEnabled { get; set; }

        public virtual Func<TMessage, PipelineContext, MessageReceiver> MessageReceiver { get; set; }
        public virtual Func<TMessage, PipelineContext, MessageSender> MessageSender { get; set; }
        public virtual Func<TMessage, PipelineContext, Message> Message { get; set; }

        // the event that is raised after the message is received (allows using the message to modify TMessage)
        public virtual Func<Message, PipelineContext, TMessage, Task> Received { get; set; }
    }
}
