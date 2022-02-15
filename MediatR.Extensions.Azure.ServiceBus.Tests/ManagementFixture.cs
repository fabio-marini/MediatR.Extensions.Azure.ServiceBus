﻿using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
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
            var runtimeInfo = await managementClient.GetQueueRuntimeInfoAsync(queuePath);

            runtimeInfo.MessageCount.Should().Be(expectedCount);
        }

        public async Task TopicIsRecreated(string topicPath, string subscriptionName)
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

            var defaultRuleDescription = new RuleDescription
            {
                Filter = new CorrelationFilter(subscriptionName)
            };

            await managementClient.CreateSubscriptionAsync(subscriptionDescription, defaultRuleDescription);
        }

        public async Task SubscriptionHasMessages(string topicPath, string subscriptionName, int expectedCount)
        {
            var runtimeInfo = await managementClient.GetSubscriptionRuntimeInfoAsync(topicPath, subscriptionName);

            runtimeInfo.MessageCount.Should().Be(expectedCount);
        }
    }
}
