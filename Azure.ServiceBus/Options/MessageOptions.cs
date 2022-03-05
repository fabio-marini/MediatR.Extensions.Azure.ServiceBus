using Azure.Messaging.ServiceBus;
using MediatR.Extensions.Abstractions;
using System;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus
{
    public class MessageOptions<TMessage>
    {
        public virtual bool IsEnabled { get; set; }

        public virtual ServiceBusReceiver Receiver { get; set; }
        public virtual ServiceBusSender Sender { get; set; }
        public virtual Func<TMessage, PipelineContext, ServiceBusMessage> Message { get; set; }

        // the event that is raised after the message is received (allows using the message to modify TMessage)
        public virtual Func<ServiceBusReceivedMessage, PipelineContext, TMessage, Task> Received { get; set; }

        // the event that is raised after the message is scheduled (allows retrieving the message sequence number)
        public virtual Func<long, ServiceBusMessage, PipelineContext, TMessage, Task> Scheduled { get; set; }

        // a delegate that is used to determine the sequence number of the scheduled message to be cancelled
        public virtual Func<PipelineContext, TMessage, long> SequenceNumber { get; set; }
    }
}
