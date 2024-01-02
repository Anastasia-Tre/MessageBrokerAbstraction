using System.Reflection;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageBroker.Core.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services,
        Action<MessageBrokerBuilder>? configure = null)
    {
        services.AddMessageBroker();

        if (configure is not null)
        {
            var mbb = (MessageBrokerBuilder)services
                .FirstOrDefault(x =>
                    x.ServiceType == typeof(MessageBrokerBuilder) &&
                    x.ImplementationInstance != null)?.ImplementationInstance!;
            if (mbb is not null)
            {
                configure(mbb);
                foreach (var action in mbb.PostConfigurationActions)
                    action(services);
            }
        }

        return services;
    }

    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services)
    {
        if (services.All(x => x.ServiceType != typeof(MessageBrokerBuilder)))
        {
            var mbb = MessageBrokerBuilder.Create();
            services.Add(ServiceDescriptor.Singleton(mbb));
        }

        services.TryAddSingleton(svp =>
        {
            var mbb = svp.GetRequiredService<MessageBrokerBuilder>();
            mbb.WithDependencyResolver(svp);
            return mbb.Settings;
        });

        services.TryAddSingleton(svp =>
        {
            var mbb = svp.GetRequiredService<MessageBrokerBuilder>();
            var messageBrokerSettings =
                svp.GetRequiredService<MessageBrokerSettings>();
            return mbb.Build();
        });

        services.TryAddTransient<ISubscriberControl>(svp =>
            svp.GetRequiredService<IMessageBroker>());
        services.AddHostedService<MessageBrokerHostedService>();

        return services;
    }

    public static void RegisterAllTypes<T>(this IServiceCollection services,
        Assembly[] assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var typesFromAssemblies = assemblies.SelectMany(a =>
            a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
        foreach (var type in typesFromAssemblies)
            services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
    }

    public static MessageBrokerBuilder AddServicesFromAssembly(
        this MessageBrokerBuilder mbb,
        Assembly assembly,
        Func<Type, bool>? filter = null,
        ServiceLifetime subscriberLifetime = ServiceLifetime.Transient)
    {
        mbb.PostConfigurationActions.Add(services =>
        {
            var typesFromAssemblies = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces().Contains(typeof(ISubscriber)))
                .Where(t => filter == null || filter(t))
                .ToList();

            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(ISubscriber), type,
                    subscriberLifetime));
                services.TryAdd(
                    ServiceDescriptor.Describe(type, type, subscriberLifetime));
            }
        });

        return mbb;
    }

    public static MessageBrokerBuilder AddServicesFromAssemblyContaining<T>(
        this MessageBrokerBuilder mbb,
        Func<Type, bool>? filter = null,
        ServiceLifetime subscriberLifetime = ServiceLifetime.Transient)
    {
        return mbb.AddServicesFromAssembly(typeof(T).Assembly, filter,
            subscriberLifetime);
    }
}
