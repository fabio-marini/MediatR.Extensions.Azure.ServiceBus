﻿using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests.Core
{
    [Trait("TestCategory", "Integration"), Collection("TopicTests")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class TopicExtensionsTests
    {
        private readonly ITestOutputHelper log;
        private readonly IConfiguration cfg;

        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        private const string topicPath = "mediator-topic";

        public TopicExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;
            this.cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ManagementClient(connectionString));
        }

        public static IEnumerable<object[]> SubscriptionNames()
        {
            yield return new object[] { TestSubscriptions.RequestProcessor };
            yield return new object[] { TestSubscriptions.ResponseProcessor };
            yield return new object[] { TestSubscriptions.RequestBehavior };
            yield return new object[] { TestSubscriptions.ResponseBehavior };
        }

        [Theory(DisplayName = "Topic and subscriptions are recreated"), MemberData(nameof(SubscriptionNames))]
        public async Task Step01(string subscriptionName) => await managementFixture.TopicIsRecreated(topicPath, subscriptionName);

        [Theory(DisplayName = "Send extensions are executed"), MemberData(nameof(SubscriptionNames))]
        public async Task Step02(string subscriptionName)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<MessageSender>(sp => new MessageSender(connectionString, topicPath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddMessageOptions<EchoRequest, EchoResponse>()
                .AddSendMessageExtensions<EchoRequest, EchoResponse>()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Subscriptions have messages"), MemberData(nameof(SubscriptionNames))]
        public async Task Step03(string subscriptionName) => await managementFixture.SubscriptionHasMessages(topicPath, subscriptionName, 4);

        [Theory(DisplayName = "Receive extensions are executed"), MemberData(nameof(SubscriptionNames))]
        public async Task Step04(string subscriptionName)
        {
            var entityPath = EntityNameHelper.FormatSubscriptionPath(topicPath, subscriptionName);

            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<MessageReceiver>(sp => new MessageReceiver(connectionString, entityPath, ReceiveMode.ReceiveAndDelete))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddMessageOptions<EchoRequest, EchoResponse>()
                .AddReceiveMessageExtensions<EchoRequest, EchoResponse>(subscriptionName)

                .BuildServiceProvider();

            using var cancelSource = new CancellationTokenSource(3000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Subscriptions have messages"), MemberData(nameof(SubscriptionNames))]
        public async Task Step05(string subscriptionName) => await managementFixture.SubscriptionHasMessages(topicPath, subscriptionName, 0);
    }
}