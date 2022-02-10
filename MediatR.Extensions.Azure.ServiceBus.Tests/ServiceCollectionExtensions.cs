using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace MediatR.Extensions.Azure.ServiceBus.Tests
{
    //public static class ServiceCollectionExtensions
    //{
    //    public static IServiceCollection AddQueueExtensions<TRequest, TResponse>(this IServiceCollection services, Func<IServiceProvider, IOptions<QueueOptions<TRequest>>> options) where TRequest : IRequest<TResponse>
    //    {
    //        return services

    //            .AddTransient<IOptions<QueueOptions<TRequest>>>(sp => options(sp))
    //            .AddTransient<SendQueueMessageCommand<TRequest>>()

    //            .AddTransient<SendQueueMessageRequestBehavior<TRequest, TResponse>>()
    //            .AddTransient<SendQueueMessageRequestProcessor<TRequest>>()

    //            ;
    //    }

    //    public static IServiceCollection AddQueueExtensions<TRequest, TResponse>(this IServiceCollection services, Func<IServiceProvider, IOptions<QueueOptions<TResponse>>> options) where TRequest : IRequest<TResponse>
    //    {
    //        return services

    //            .AddTransient<IOptions<QueueOptions<TResponse>>>(sp => options(sp))
    //            .AddTransient<SendQueueMessageCommand<TResponse>>()

    //            .AddTransient<SendQueueMessageResponseBehavior<TRequest, TResponse>>()
    //            .AddTransient<SendQueueMessageResponseProcessor<TRequest, TResponse>>()

    //            ;
    //    }

    //    public static IServiceCollection AddTopicExtensions<TRequest, TResponse>(this IServiceCollection services, Func<IServiceProvider, IOptions<TopicOptions<TRequest>>> options) where TRequest : IRequest<TResponse>
    //    {
    //        return services

    //            .AddTransient<IOptions<TopicOptions<TRequest>>>(sp => options(sp))
    //            .AddTransient<SendTopicMessageCommand<TRequest>>()

    //            .AddTransient<SendTopicMessageRequestBehavior<TRequest, TResponse>>()
    //            .AddTransient<SendTopicMessageRequestProcessor<TRequest>>()

    //            ;
    //    }

    //    public static IServiceCollection AddTopicExtensions<TRequest, TResponse>(this IServiceCollection services, Func<IServiceProvider, IOptions<TopicOptions<TResponse>>> options) where TRequest : IRequest<TResponse>
    //    {
    //        return services

    //            .AddTransient<IOptions<TopicOptions<TResponse>>>(sp => options(sp))
    //            .AddTransient<SendTopicMessageCommand<TResponse>>()

    //            .AddTransient<SendTopicMessageResponseBehavior<TRequest, TResponse>>()
    //            .AddTransient<SendTopicMessageResponseProcessor<TRequest, TResponse>>()

    //            ;
    //    }
    //}
}
