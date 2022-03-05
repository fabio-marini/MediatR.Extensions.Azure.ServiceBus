namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    // TODO: update storage extension commands and tests to use Invoke() on delegates?
    // TODO: ideally all options should be verified - use mock behavior strict?

    // TODO: docs + nuget feed for GitHub projects?
    // TODO: rename core project (drop core) + shorten solution names (drop MediatR.Extensions)?

    public class TestEntities
    {
        // these are used by the core/cancel extensions
        public const string QueuePath = "mediator-queue";
        public const string TopicPath = "mediator-topic";
        public const string SubscriptionName = "mediator-subscription";
    }
}
