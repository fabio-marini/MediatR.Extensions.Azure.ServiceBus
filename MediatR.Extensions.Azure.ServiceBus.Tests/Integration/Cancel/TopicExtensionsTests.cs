using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FluentAssertions;
using MediatR.Extensions.Abstractions;
using MediatR.Extensions.Azure.Storage.Examples;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MediatR.Extensions.Azure.ServiceBus.Tests.Cancel
{
    [Trait("TestCategory", "Integration"), Collection("TopicTests")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class TopicExtensionsTests : IClassFixture<SequenceNumbersFixture>
    {
        private readonly ITestOutputHelper log;
        private readonly SequenceNumbersFixture fix;
        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        public TopicExtensionsTests(ITestOutputHelper log, SequenceNumbersFixture fix)
        {
            this.log = log;
            this.fix = fix;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ServiceBusAdministrationClient(connectionString));
        }

        [Fact(DisplayName = "01. Topics are recreated")]
        public async Task Step01() => await managementFixture.TopicIsRecreated(TestEntities.TopicPath, TestEntities.SubscriptionName);

        [Fact(DisplayName = "02. Send extensions are executed")]
        public async Task Step02()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<ServiceBusSender>(sp => new ServiceBusClient(connectionString).CreateSender(TestEntities.TopicPath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddOptions<TestOutputLoggerOptions>().Configure(opt => opt.MinimumLogLevel = LogLevel.Information).Services
                .AddMessageOptions()
                .AddScheduleMessageExtensions()
                .AddScoped<PipelineContext>()

                .BuildServiceProvider();

            var ctx = serviceProvider.GetRequiredService<PipelineContext>();

            ctx.Add(ContextKeys.EnqueueTimeUtc, DateTimeOffset.UtcNow.AddSeconds(60));

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);

            ctx.Should().ContainKey(ContextKeys.SequenceNumbers);
            ctx[ContextKeys.SequenceNumbers].As<Queue<long>>().Should().HaveCount(4);

            fix.TestTopic = (Queue<long>)ctx[ContextKeys.SequenceNumbers];
        }

        [Fact(DisplayName = "03. Topic has scheduled messages")]
        public async Task Step03() => await managementFixture.TopicHasScheduledMessages(TestEntities.TopicPath, 4);

        [Fact(DisplayName = "04. Cancel extensions are executed")]
        public async Task Step04()
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<ServiceBusSender>(sp => new ServiceBusClient(connectionString).CreateSender(TestEntities.TopicPath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddOptions<TestOutputLoggerOptions>().Configure(opt => opt.MinimumLogLevel = LogLevel.Information).Services
                .AddMessageOptions()
                .AddCancelMessageExtensions()
                .AddScoped<PipelineContext>()

                .BuildServiceProvider();

            var ctx = serviceProvider.GetRequiredService<PipelineContext>();

            ctx.Add(ContextKeys.SequenceNumbers, fix.TestTopic);

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Fact(DisplayName = "05. Topic has scheduled messages")]
        public async Task Step05() => await managementFixture.TopicHasScheduledMessages(TestEntities.TopicPath, 0);
    }
}
