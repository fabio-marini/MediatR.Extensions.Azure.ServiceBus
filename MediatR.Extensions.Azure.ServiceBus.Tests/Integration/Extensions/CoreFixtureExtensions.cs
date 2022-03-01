using Azure.Messaging.ServiceBus;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    public static class CoreFixtureExtensions
    {
        public static IServiceCollection AddMessageOptions(this IServiceCollection services) => services.AddMessageOptions<EchoRequest, EchoResponse>();

        public static IServiceCollection AddSendMessageExtensions(this IServiceCollection services) => services.AddSendMessageExtensions<EchoRequest, EchoResponse>();

        public static IServiceCollection AddCancelMessageExtensions(this IServiceCollection services) => services.AddCancelMessageExtensions<EchoRequest, EchoResponse>();

        public static IServiceCollection AddReceiveMessageExtensions(this IServiceCollection services) => services.AddReceiveMessageExtensions<EchoRequest, EchoResponse>();

        private static IServiceCollection AddMessageOptions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddOptions<MessageOptions<TRequest>>("Processors")
                .Configure<IServiceProvider>((Action<MessageOptions<TRequest>, IServiceProvider>)((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.Receiver = (req, ctx) => svc.GetRequiredService<ServiceBusReceiver>();
                    opt.Sender = (req, ctx) => svc.GetRequiredService<ServiceBusSender>();
                    opt.Message = (req, ctx) => new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                }))
                .Services

                .AddOptions<MessageOptions<TResponse>>("Processors")
                .Configure<IServiceProvider>((Action<MessageOptions<TResponse>, IServiceProvider>)((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.Receiver = (req, ctx) => svc.GetRequiredService<ServiceBusReceiver>();
                    opt.Sender = (req, ctx) => svc.GetRequiredService<ServiceBusSender>();
                    opt.Message = (req, ctx) => new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                }))
                .Services

                .AddOptions<MessageOptions<TRequest>>("Behaviors")
                .Configure<IServiceProvider>((Action<MessageOptions<TRequest>, IServiceProvider>)((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.Receiver = (req, ctx) => svc.GetRequiredService<ServiceBusReceiver>();
                    opt.Sender = (req, ctx) => svc.GetRequiredService<ServiceBusSender>();
                    opt.Message = (req, ctx) => new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                }))
                .Services

                .AddOptions<MessageOptions<TResponse>>("Behaviors")
                .Configure<IServiceProvider>((Action<MessageOptions<TResponse>, IServiceProvider>)((opt, svc) =>
                {
                    opt.IsEnabled = true;
                    opt.Receiver = (req, ctx) => svc.GetRequiredService<ServiceBusReceiver>();
                    opt.Sender = (req, ctx) => svc.GetRequiredService<ServiceBusSender>();
                    opt.Message = (req, ctx) => new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
                }))
                .Services

                ;
        }

        private static IServiceCollection AddSendMessageExtensions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddTransient<IRequestPreProcessor<TRequest>, SendMessageRequestProcessor<TRequest>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TRequest>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageRequestProcessor<TRequest>>(sp, cmd);
                })
                .AddTransient<IRequestPostProcessor<TRequest, TResponse>, SendMessageResponseProcessor<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TResponse>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageResponseProcessor<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, SendMessageRequestBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TRequest>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageRequestBehavior<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, SendMessageResponseBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TResponse>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<SendMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<SendMessageResponseBehavior<TRequest, TResponse>>(sp, cmd);
                })

                ;
        }

        private static IServiceCollection AddCancelMessageExtensions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddTransient<IRequestPreProcessor<TRequest>, CancelMessageRequestProcessor<TRequest>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TRequest>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<CancelMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<CancelMessageRequestProcessor<TRequest>>(sp, cmd);
                })
                .AddTransient<IRequestPostProcessor<TRequest, TResponse>, CancelMessageResponseProcessor<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TResponse>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<CancelMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<CancelMessageResponseProcessor<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, CancelMessageRequestBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TRequest>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<CancelMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<CancelMessageRequestBehavior<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, CancelMessageResponseBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TResponse>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<CancelMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<CancelMessageResponseBehavior<TRequest, TResponse>>(sp, cmd);
                })

                ;
        }

        private static IServiceCollection AddReceiveMessageExtensions<TRequest, TResponse>(this IServiceCollection services) where TRequest : IRequest<TResponse>
        {
            return services

                .AddTransient<IRequestPreProcessor<TRequest>, ReceiveMessageRequestProcessor<TRequest>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TRequest>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<ReceiveMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<ReceiveMessageRequestProcessor<TRequest>>(sp, cmd);
                })
                .AddTransient<IRequestPostProcessor<TRequest, TResponse>, ReceiveMessageResponseProcessor<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TResponse>>>().Get("Processors");

                    var cmd = ActivatorUtilities.CreateInstance<ReceiveMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<ReceiveMessageResponseProcessor<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, ReceiveMessageRequestBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TRequest>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<ReceiveMessageCommand<TRequest>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<ReceiveMessageRequestBehavior<TRequest, TResponse>>(sp, cmd);
                })
                .AddTransient<IPipelineBehavior<TRequest, TResponse>, ReceiveMessageResponseBehavior<TRequest, TResponse>>(sp =>
                {
                    var opt = sp.GetRequiredService<IOptionsSnapshot<MessageOptions<TResponse>>>().Get("Behaviors");

                    var cmd = ActivatorUtilities.CreateInstance<ReceiveMessageCommand<TResponse>>(sp, Options.Create(opt));

                    return ActivatorUtilities.CreateInstance<ReceiveMessageResponseBehavior<TRequest, TResponse>>(sp, cmd);
                })

                ;
        }
    }
}
