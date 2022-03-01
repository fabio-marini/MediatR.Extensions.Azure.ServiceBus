﻿using Azure.Messaging.ServiceBus;
using MediatR.Extensions.Abstractions;
using System;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class MessageOptions<TMessage>
    {
        public virtual bool IsEnabled { get; set; }

        public virtual Func<TMessage, PipelineContext, ServiceBusReceiver> MessageReceiver { get; set; }
        public virtual Func<TMessage, PipelineContext, ServiceBusSender> MessageSender { get; set; }
        public virtual Func<TMessage, PipelineContext, ServiceBusMessage> Message { get; set; }

        // the event that is raised after the message is received (allows using the message to modify TMessage)
        public virtual Func<ServiceBusReceivedMessage, PipelineContext, TMessage, Task> Received { get; set; }
    }
}
