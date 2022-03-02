namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    // FIXME: BlobClient is a delegate, but table and queue clients are instances - what should SB topic and queue clients be?!?
    // TODO: commands unit tests + docs
    // TODO: rename core project (drop core)
    // TODO: change send commands to use default Message, i.e. serialize req using json...

    // TODO: list contoso/fabrikam examples (not integration tests, include some that use the DLQ)

    public class TestEntities
    {
        // these are used by the message/session handler extensions
        public const string RequestProcessor = "request-processor";
        public const string ResponseProcessor = "response-processor";
        public const string RequestBehavior = "request-behavior";
        public const string ResponseBehavior = "response-behavior";

        // these are used by the core/cancel extensions
        public const string QueuePath = "mediator-queue";
        public const string TopicPath = "mediator-topic";
        public const string SubscriptionName = "mediator-subscription";
    }
}
