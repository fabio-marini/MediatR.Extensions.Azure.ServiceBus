using System.Collections.Generic;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class SequenceNumbersFixture
    {
        public SequenceNumbersFixture()
        {
            TestQueue = new Queue<long>();
            TestTopic = new Queue<long>();
        }

        public Queue<long> TestQueue { get; set; }
        public Queue<long> TestTopic { get; set; }
    }
}
