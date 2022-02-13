using MediatR.Pipeline;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public static class TopicFixtureExtensions
    {
        public static IServiceCollection AddTopicOptions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddOptions<TopicOptions<TRequest>>("Processors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.TopicClient = (req, ctx) => svc.GetRequiredService<TopicClient>();
                    opt.Message = (req, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                })
                .Services

                .AddOptions<TopicOptions<TResponse>>("Processors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.TopicClient = (req, ctx) => svc.GetRequiredService<TopicClient>();
                    opt.Message = (res, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                })
                .Services

                .AddOptions<TopicOptions<TRequest>>("Behaviors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.TopicClient = (req, ctx) => svc.GetRequiredService<TopicClient>();
                    opt.Message = (req, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                })
                .Services

                .AddOptions<TopicOptions<TResponse>>("Behaviors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.TopicClient = (req, ctx) => svc.GetRequiredService<TopicClient>();
                    opt.Message = (res, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                })
                .Services

                ;
        }

        public static IServiceCollection AddSubscriptionOptions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddOptions<SubscriptionOptions<TRequest>>("Processors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.SubscriptionClient = (req, ctx) => svc.GetRequiredService<SubscriptionClient>();
                    opt.Message = (req, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                })
                .Services

                .AddOptions<SubscriptionOptions<TResponse>>("Processors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.SubscriptionClient = (req, ctx) => svc.GetRequiredService<SubscriptionClient>();
                    opt.Message = (res, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                })
                .Services

                .AddOptions<SubscriptionOptions<TRequest>>("Behaviors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.SubscriptionClient = (req, ctx) => svc.GetRequiredService<SubscriptionClient>();
                    opt.Message = (req, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                })
                .Services

                .AddOptions<SubscriptionOptions<TResponse>>("Behaviors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.SubscriptionClient = (req, ctx) => svc.GetRequiredService<SubscriptionClient>();
                    opt.Message = (res, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                })
                .Services

                ;
        }

        public static IServiceCollection AddSendTopicMessageExtensions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddTransient<IRequestPreProcessor<TRequest>, SendTopicMessageRequestProcessor<TRequest>>(sp =>
                 {
                     var opt = sp.GetRequiredService<IOptionsSnapshot<TopicOptions<TRequest>>>().Get("Processors");

                     var cmd = ActivatorUtilities.CreateInstance<SendTopicMessageCommand<TRequest>>(sp, Options.Create(opt));

                     return ActivatorUtilities.CreateInstance<SendTopicMessageRequestProcessor<TRequest>>(sp, cmd);
                 })
                .AddTransient<IRequestPostProcessor<TRequest, TResponse>, SendTopicMessageResponseProcessor<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<TopicOptions<TResponse>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<SendTopicMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendTopicMessageResponseProcessor<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, SendTopicMessageRequestBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<TopicOptions<TRequest>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<SendTopicMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendTopicMessageRequestBehavior<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, SendTopicMessageResponseBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<TopicOptions<TResponse>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<SendTopicMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendTopicMessageResponseBehavior<TRequest, TResponse>>(sp, cmd);
                })

                ;
        }

        public static IServiceCollection AddReceiveSubscriptionMessageExtensions<TRequest, TResponse>(this IServiceCollection services, string subscriptionName) where TRequest : IRequest<TResponse>
        {
            // only execute one receive extension at the time, otherwise the first will consume the
            // cancellation token and when the next 3 start they will be cancelled straight away...
            switch (subscriptionName)
            {
                case TestSubscriptions.RequestProcessor:
                    services.AddTransient<IRequestPreProcessor<TRequest>, ReceiveSubscriptionMessageRequestProcessor<TRequest>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SubscriptionOptions<TRequest>>>().Get("Processors");

                        var cmd = ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageCommand<TRequest>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageRequestProcessor<TRequest>>(sp, cmd);
                    });
                    break;

                case TestSubscriptions.ResponseProcessor:
                    services.AddTransient<IRequestPostProcessor<TRequest, TResponse>, ReceiveSubscriptionMessageResponseProcessor<TRequest, TResponse>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SubscriptionOptions<TResponse>>>().Get("Processors");

                        var cmd = ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageCommand<TResponse>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageResponseProcessor<TRequest, TResponse>>(sp, cmd);
                    });
                    break;

                case TestSubscriptions.RequestBehavior:
                    services.AddTransient<IPipelineBehavior<TRequest, TResponse>, ReceiveSubscriptionMessageRequestBehavior<TRequest, TResponse>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SubscriptionOptions<TRequest>>>().Get("Behaviors");

                        var cmd = ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageCommand<TRequest>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageRequestBehavior<TRequest, TResponse>>(sp, cmd);
                    });
                    break;

                case TestSubscriptions.ResponseBehavior:
                    services.AddTransient<IPipelineBehavior<TRequest, TResponse>, ReceiveSubscriptionMessageResponseBehavior<TRequest, TResponse>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<SubscriptionOptions<TResponse>>>().Get("Behaviors");

                        var cmd = ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageCommand<TResponse>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<ReceiveSubscriptionMessageResponseBehavior<TRequest, TResponse>>(sp, cmd);
                    });
                    break;
            }

            return services;

        }
    }
}
