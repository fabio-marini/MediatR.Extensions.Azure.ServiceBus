namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    // TODO: how to refactor the RegisterMessageHandler theories as facts and use a single entity
    //       (each extension will consume all messages from a queue/sub so needs to be executed independently)

    // TODO: implement session
    // TODO: commands unit tests + docs

    // TODO: list contoso/fabrikam examples (not integration tests, include some that use theDLQ)

    // TODO: update storage test fixtures so tables/containers are deleted on dispose?
    // FIXME: BlobClient is a delegate, but AS table and queue clients are instances - what should SB topic and queue clients be?!?
    // TODO: confirm core doesn't support: sessions, manual complete
    // TODO: refactor storage DI extensions to be non-generic...

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
