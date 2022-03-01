﻿using FluentAssertions;
using MediatR.Extensions.Azure.Storage.Examples;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests.Handlers
{
    [Trait("TestCategory", "Integration"), Collection("TopicTests")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class TopicExtensionsTests
    {
        private readonly ITestOutputHelper log;
        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        public TopicExtensionsTests(ITestOutputHelper log)
        {
            this.log = log;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ManagementClient(connectionString));
        }

        public static IEnumerable<object[]> SubscriptionNames()
        {
            yield return new object[] { TestEntities.RequestProcessor };
            yield return new object[] { TestEntities.ResponseProcessor };
            yield return new object[] { TestEntities.RequestBehavior };
            yield return new object[] { TestEntities.ResponseBehavior };
        }

        [Theory(DisplayName = "Topic and subscriptions are recreated"), MemberData(nameof(SubscriptionNames))]
        public async Task Step01(string subscriptionName)
        {
            var defaultRule = new RuleDescription
            {
                Filter = new CorrelationFilter(subscriptionName)
            };

            await managementFixture.TopicIsRecreated(TestEntities.TopicPath, subscriptionName, defaultRule);
        }

        [Theory(DisplayName = "Send extensions are executed"), MemberData(nameof(SubscriptionNames))]
        public async Task Step02(string subscriptionName)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<TopicClient>(sp => new TopicClient(connectionString, TestEntities.TopicPath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions { MinimumLogLevel = LogLevel.Information })
                .AddTopicOptions()
                .AddSendTopicMessageExtensions()

                .BuildServiceProvider();

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Subscriptions have messages"), MemberData(nameof(SubscriptionNames))]
        public async Task Step03(string subscriptionName) => await managementFixture.SubscriptionHasMessages(TestEntities.TopicPath, subscriptionName, 4);

        [Theory(DisplayName = "Receive extensions are executed"), MemberData(nameof(SubscriptionNames))]
        public async Task Step04(string subscriptionName)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<SubscriptionClient>(sp => new SubscriptionClient(connectionString, TestEntities.TopicPath, subscriptionName))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions { MinimumLogLevel = LogLevel.Information })
                .AddSubscriptionOptions()
                .AddReceiveSubscriptionMessageExtensions(subscriptionName)

                .BuildServiceProvider();

            using var cancelSource = new CancellationTokenSource(3000);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default, cancelSource.Token);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Subscriptions have messages"), MemberData(nameof(SubscriptionNames))]
        public async Task Step05(string subscriptionName) => await managementFixture.SubscriptionHasMessages(TestEntities.TopicPath, subscriptionName, 0);
    }
}