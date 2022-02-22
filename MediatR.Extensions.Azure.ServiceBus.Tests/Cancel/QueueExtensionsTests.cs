using FluentAssertions;
using MediatR.Extensions.Abstractions;
using MediatR.Extensions.Azure.Storage.Examples;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
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
    [Trait("TestCategory", "Integration"), Collection("QueueTests")]
    [TestCaseOrderer("MediatR.Extensions.Tests.TestMethodNameOrderer", "Timeless.Testing.Xunit")]
    public class QueueExtensionsTests : IClassFixture<SequenceNumbersFixture>
    {
        private readonly ITestOutputHelper log;
        private readonly SequenceNumbersFixture fix;
        private readonly string connectionString;
        private readonly ManagementFixture managementFixture;

        public QueueExtensionsTests(ITestOutputHelper log, SequenceNumbersFixture fix)
        {
            this.log = log;
            this.fix = fix;

            var cfg = new ConfigurationBuilder().AddUserSecrets(this.GetType().Assembly).Build();

            connectionString = cfg.GetValue<string>("AzureWebJobsServiceBus");

            managementFixture = new ManagementFixture(new ManagementClient(connectionString));
        }

        public static IEnumerable<object[]> QueueNames()
        {
            yield return new object[] { TestEntities.RequestProcessor };
            yield return new object[] { TestEntities.ResponseProcessor };
            yield return new object[] { TestEntities.RequestBehavior };
            yield return new object[] { TestEntities.ResponseBehavior };
        }

        [Theory(DisplayName = "Queues are recreated"), MemberData(nameof(QueueNames))]
        public async Task Step01(string queuePath) => await managementFixture.QueueIsRecreated(queuePath);

        [Theory(DisplayName = "Send extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step02(string queuePath)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<MessageSender>(sp => new MessageSender(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions
                {
                    MinimumLogLevel = LogLevel.Debug
                })
                .AddMessageOptions()
                .AddSendMessageExtensions()
                .AddScoped<PipelineContext>()

                .BuildServiceProvider();

            var ctx = serviceProvider.GetRequiredService<PipelineContext>();

            ctx.Add(ContextKeys.EnqueueTimeUtc, DateTimeOffset.UtcNow.AddSeconds(60));

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);

            ctx.Should().ContainKey(ContextKeys.SequenceNumbers);
            ctx[ContextKeys.SequenceNumbers].As<Queue<long>>().Should().HaveCount(4);

            switch (queuePath)
            {
                case TestEntities.RequestProcessor:
                    fix.RequestProcessor = (Queue<long>)ctx[ContextKeys.SequenceNumbers];
                    break;

                case TestEntities.ResponseProcessor:
                    fix.ResponseProcessor = (Queue<long>)ctx[ContextKeys.SequenceNumbers];
                    break;

                case TestEntities.RequestBehavior:
                    fix.RequestBehavior = (Queue<long>)ctx[ContextKeys.SequenceNumbers];
                    break;

                case TestEntities.ResponseBehavior:
                    fix.ResponseBehavior = (Queue<long>)ctx[ContextKeys.SequenceNumbers];
                    break;
            }
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Step03(string queuePath) => await managementFixture.QueueHasMessages(queuePath, 4);

        [Theory(DisplayName = "Cancel extensions are executed"), MemberData(nameof(QueueNames))]
        public async Task Step04(string queuePath)
        {
            var serviceProvider = new ServiceCollection()

                .AddMediatR(this.GetType())
                .AddTransient<MessageSender>(sp => new MessageSender(connectionString, queuePath))
                .AddTransient<ITestOutputHelper>(sp => log)
                .AddTransient<ILogger, TestOutputLogger>()
                .AddTransient<TestOutputLoggerOptions>(sp => new TestOutputLoggerOptions
                {
                    MinimumLogLevel = LogLevel.Debug
                })
                .AddMessageOptions()
                .AddCancelMessageExtensions()
                .AddScoped<PipelineContext>()

                .BuildServiceProvider();

            var ctx = serviceProvider.GetRequiredService<PipelineContext>();

            switch (queuePath)
            {
                case TestEntities.RequestProcessor:
                    ctx.Add(ContextKeys.SequenceNumbers, fix.RequestProcessor);
                    break;

                case TestEntities.ResponseProcessor:
                    ctx.Add(ContextKeys.SequenceNumbers, fix.ResponseProcessor);
                    break;

                case TestEntities.RequestBehavior:
                    ctx.Add(ContextKeys.SequenceNumbers, fix.RequestBehavior);
                    break;

                case TestEntities.ResponseBehavior:
                    ctx.Add(ContextKeys.SequenceNumbers, fix.ResponseBehavior);
                    break;
            }

            var med = serviceProvider.GetRequiredService<IMediator>();

            var res = await med.Send(EchoRequest.Default);

            res.Message.Should().Be(EchoRequest.Default.Message);
        }

        [Theory(DisplayName = "Queues have messages"), MemberData(nameof(QueueNames))]
        public async Task Step05(string queuePath) => await managementFixture.QueueHasMessages(queuePath, 0);
    }
}
