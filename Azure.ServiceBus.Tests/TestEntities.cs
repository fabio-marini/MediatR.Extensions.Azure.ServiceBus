namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    // TODO: simplify unit tests: call Verify() on all setup and remove manual verification + only verify that command method is called, e.g. SendAsync
    // TODO: print all mock invocations?
    public class TestEntities
    {
        // these are used by the core/cancel extensions
        public const string QueuePath = "mediator-queue";
        public const string TopicPath = "mediator-topic";
        public const string SubscriptionName = "mediator-subscription";
    }
}
