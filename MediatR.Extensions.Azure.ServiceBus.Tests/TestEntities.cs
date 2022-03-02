namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    // TODO: how to cancel a specific message - create pipeline that schedules 4 messages, but only cancels 2 specific ones...

    // TODO: manage the list of messages to be cancelled (i.e. seq numbers) using separate components in the pipeline
    //       - scenario 1: schedule and cancel in the same pipeline - can use context
    //       - scenario 2: schedule and cancel in different pipelines - use persistence
    //       - scenario 3: cancel scheduled messages based on request (delete from persistence store or cancel message)

    // TODO: update storage extension commands and tests to use Invoke() on delegates?

    // TODO: test cancel with different enqueue times
    // TODO: integration test pattern - how to execute entire test with different parameters, i.e.
    //       steps 1/2/3/4/5 with param1, then steps 1/2/3/4/5 with param2 - class fixture or inherit from a base class?
    //       using theories => step 1 with param1/param2, step 2 with param1/param2, step 3 with param1/param2...

    // TODO: commands unit tests + docs
    // TODO: rename core project (drop core) + shorten solution names (drop MediatR.Extensions)?

    // TODO: list contoso/fabrikam examples (include DLQ, optional message delegate)

    public class TestEntities
    {
        // these are used by the core/cancel extensions
        public const string QueuePath = "mediator-queue";
        public const string TopicPath = "mediator-topic";
        public const string SubscriptionName = "mediator-subscription";
    }
}
