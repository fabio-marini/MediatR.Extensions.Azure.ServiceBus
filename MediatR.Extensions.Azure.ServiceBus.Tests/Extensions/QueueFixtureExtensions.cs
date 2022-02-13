using MediatR.Extensions.Azure.ServiceBus.Queues;
using MediatR.Pipeline;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public static class QueueFixtureExtensions
    {
        public static IServiceCollection AddQueueOptions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddOptions<QueueOptions<TRequest>>("Processors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.QueueClient = (req, ctx) => svc.GetRequiredService<QueueClient>();
                    opt.Message = (req, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                })
                .Services

                .AddOptions<QueueOptions<TResponse>>("Processors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.QueueClient = (res, ctx) => svc.GetRequiredService<QueueClient>();
                    opt.Message = (res, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                })
                .Services

                .AddOptions<QueueOptions<TRequest>>("Behaviors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.QueueClient = (req, ctx) => svc.GetRequiredService<QueueClient>();
                    opt.Message = (req, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                })
                .Services

                .AddOptions<QueueOptions<TResponse>>("Behaviors")
                .Configure<IServiceProvider>((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.QueueClient = (res, ctx) => svc.GetRequiredService<QueueClient>();
                    opt.Message = (res, ctx) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                })
                .Services

                ;
        }

        public static IServiceCollection AddSendQueueMessageExtensions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddTransient<IRequestPreProcessor<TRequest>, SendMessageRequestProcessor<TRequest>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TRequest>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageRequestProcessor<TRequest>>(sp, cmd);
                })
                .AddTransient<IRequestPostProcessor<TRequest, TResponse>, SendMessageResponseProcessor<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TResponse>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageResponseProcessor<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, SendQueueMessageRequestBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TRequest>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendQueueMessageRequestBehavior<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, SendMessageResponseBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TResponse>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageResponseBehavior<TRequest, TResponse>>(sp, cmd);
                })

                ;
        }

        public static IServiceCollection AddReceiveQueueMessageExtensions<TRequest, TResponse>(this IServiceCollection services, string queuePath) where TRequest : IRequest<TResponse>
        {
            // only execute one receive extension at the time, otherwise the first will consume the
            // cancellation token and when the next 3 start they will be cancelled straight away...
            switch (queuePath)
            {
                case TestQueues.RequestProcessor:
                    services.AddTransient<IRequestPreProcessor<TRequest>, RegisterMessageHandlerRequestProcessor<TRequest>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TRequest>>>().Get("Processors");

                        var cmd = ActivatorUtilities.CreateInstance<RegisterMessageHandlerCommand<TRequest>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<RegisterMessageHandlerRequestProcessor<TRequest>>(sp, cmd);
                    });
                    break;

                case TestQueues.ResponseProcessor:
                    services.AddTransient<IRequestPostProcessor<TRequest, TResponse>, RegisterMessageHandlerResponseProcessor<TRequest, TResponse>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TResponse>>>().Get("Processors");

                        var cmd = ActivatorUtilities.CreateInstance<RegisterMessageHandlerCommand<TResponse>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<RegisterMessageHandlerResponseProcessor<TRequest, TResponse>>(sp, cmd);
                    });
                    break;

                case TestQueues.RequestBehavior:
                    services.AddTransient<IPipelineBehavior<TRequest, TResponse>, RegisterMessageHandlerRequestBehavior<TRequest, TResponse>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TRequest>>>().Get("Behaviors");

                        var cmd = ActivatorUtilities.CreateInstance<RegisterMessageHandlerCommand<TRequest>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<RegisterMessageHandlerRequestBehavior<TRequest, TResponse>>(sp, cmd);
                    });
                    break;

                case TestQueues.ResponseBehavior:
                    services.AddTransient<IPipelineBehavior<TRequest, TResponse>, RegisterMessageHandlerResponseBehavior<TRequest, TResponse>>(sp =>
                    {
                        var opt = sp.GetRequiredService<IOptionsSnapshot<QueueOptions<TResponse>>>().Get("Behaviors");

                        var cmd = ActivatorUtilities.CreateInstance<RegisterMessageHandlerCommand<TResponse>>(sp, Options.Create(opt));

                        return ActivatorUtilities.CreateInstance<RegisterMessageHandlerResponseBehavior<TRequest, TResponse>>(sp, cmd);
                    });
                    break;
            }

            return services;
        }
    }
}
