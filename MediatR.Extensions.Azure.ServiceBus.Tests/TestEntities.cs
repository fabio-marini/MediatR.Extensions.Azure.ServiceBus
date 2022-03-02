using MediatR.Extensions.Abstractions;
using System;
using System.Collections.Generic;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    // TODO: use ScheduleContext with strongly typed properties intead of using dictionary?

    // TODO: test cancel with different enqueue times
    // TODO: integration test pattern - how to execute entire test with different parameters, i.e.
    //       steps 1/2/3/4/5 with param1, then steps 1/2/3/4/5 with param2 - class fixture or inherit from a base class?
    //       using theories => step 1 with param1/param2, step 2 with param1/param2, step 3 with param1/param2...

    // TODO: change sender and receiver to be instances, not delegates
    // TODO: change send commands to use default Message, i.e. serialize req using json...

    // TODO: commands unit tests + docs
    // TODO: rename core project (drop core) + shorten solution names (drop MediatR.Extensions)?

    // TODO: list contoso/fabrikam examples (not integration tests, include some that use the DLQ)

    public class ScheduleContext : PipelineContext
    {
        public DateTimeOffset? EnqueueTimeUtc { get; set; }
        public Queue<long> SequenceNumbers { get; set; }
    }

    public class TestEntities
    {
        // these are used by the core/cancel extensions
        public const string QueuePath = "mediator-queue";
        public const string TopicPath = "mediator-topic";
        public const string SubscriptionName = "mediator-subscription";
    }
}
