using System.Collections.Generic;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class SequenceNumbersFixture
    {
        public SequenceNumbersFixture()
        {
            RequestProcessor = new Queue<long>();
            ResponseProcessor = new Queue<long>();
            RequestBehavior = new Queue<long>();
            ResponseBehavior = new Queue<long>();
        }

        public Queue<long> RequestProcessor { get; set; }
        public Queue<long> ResponseProcessor { get; set; }
        public Queue<long> RequestBehavior { get; set; }
        public Queue<long> ResponseBehavior { get; set; }

        public Queue<long> TestQueue { get; set; }
        public Queue<long> TestTopic { get; set; }
    }
}
