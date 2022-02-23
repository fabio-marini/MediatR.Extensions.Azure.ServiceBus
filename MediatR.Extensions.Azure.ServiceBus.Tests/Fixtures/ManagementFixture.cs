using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Polly;
using System;
using System.Threading.Tasks;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public class ManagementFixture
    {
        private readonly ManagementClient managementClient;

        public ManagementFixture(ManagementClient managementClient)
        {
            this.managementClient = managementClient;
        }

        public async Task QueueIsRecreated(string queuePath)
        {
            if (await managementClient.QueueExistsAsync(queuePath))
            {
                var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

                if (runtimeInfo.MessageCount > 0)
                {
                    // only recreate queue if it has any messages...
                    await managementClient.DeleteQueueAsync(queuePath);

                    await managementClient.CreateQueueAsync(queuePath);
                }
            }
            else
            {
                await managementClient.CreateQueueAsync(queuePath);
            }
        }

        public async Task QueueHasMessages(string queuePath, int expectedCount)
        {
            var retryPolicy = Policy.HandleResult<long>(res => res != expectedCount)
                .WaitAndRetryAsync(5, x => TimeSpan.FromMilliseconds(500));

            var messageCount = await retryPolicy.ExecuteAsync(async () =>
            {
                var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

                return runtimeInfo.MessageCount;
            });

            messageCount.Should().Be(expectedCount);
        }

        public async Task TopicHasScheduledMessages(string topicPath, int expectedCount)
        {
            var retryPolicy = Policy.HandleResult<long>(res => res != expectedCount)
                .WaitAndRetryAsync(5, x => TimeSpan.FromMilliseconds(500));

            var messageCount = await retryPolicy.ExecuteAsync(async () =>
            {
                var runtimeInfo = await managementClient.GetTopicRuntimeInfoAsync(topicPath);

                return runtimeInfo.MessageCountDetails.ScheduledMessageCount;
            });

            messageCount.Should().Be(expectedCount);
        }

        public async Task TopicIsRecreated(string topicPath, string subscriptionName, RuleDescription defaultRule = default)
        {
            if (await managementClient.TopicExistsAsync(topicPath) == false)
            {
                await managementClient.CreateTopicAsync(topicPath);
            }

            if (await managementClient.SubscriptionExistsAsync(topicPath, subscriptionName) == true)
            {
                await managementClient.DeleteSubscriptionAsync(topicPath, subscriptionName);
            }

            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName);

            await managementClient.CreateSubscriptionAsync(subscriptionDescription, defaultRule);
        }

        public async Task SubscriptionHasMessages(string topicPath, string subscriptionName, int expectedCount)
        {
            var retryPolicy = Policy.HandleResult<long>(res => res != expectedCount)
                .WaitAndRetryAsync(5, x => TimeSpan.FromMilliseconds(500));

            var messageCount = await retryPolicy.ExecuteAsync(async () =>
            {
                var runtimeInfo = await managementClient.GetSubscriptionRuntimeInfoAsync(topicPath, subscriptionName);

                return runtimeInfo.MessageCount;
            });

            messageCount.Should().Be(expectedCount);
        }
    }
}
