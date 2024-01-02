using Microsoft.Extensions.DependencyInjection;
using MessageBroker.Core.Publisher;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MessageBroker.Core.MessageBroker;
using System.Reflection;
using MessageBroker.Core.DI;

namespace MessageBroker.Core.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBroker(this IServiceCollection services, Action<MessageBrokerBuilder> configure = null)
    {
        services.AddMessageBroker();

        if (configure is not null)
        {
            // Execute the mbb setup for services registration
            var mbb = (MessageBrokerBuilder)services.FirstOrDefault(x => x.ServiceType == typeof(MessageBrokerBuilder) && x.ImplementationInstance != null)?.ImplementationInstance;
            if (mbb is not null)
            {
                configure(mbb);

                // Execute post config actions for the master broker
                foreach (var action in mbb.PostConfigurationActions)
                {
                    action(services);
                }
            }
        }

        return services;
    }

    public static IServiceCollection AddMessageBroker(this IServiceCollection services)
    {
        // MessageBrokerBuilder
        if (services.All(x => x.ServiceType != typeof(MessageBrokerBuilder)))
        {
            var mbb = MessageBrokerBuilder.Create();
            services.Add(ServiceDescriptor.Singleton(mbb));
        }

        // MessageBrokerSettings
        services.TryAddSingleton(svp =>
        {
            var mbb = svp.GetRequiredService<MessageBrokerBuilder>();
            mbb.WithDependencyResolver(svp);
            return mbb.Settings;
        });

        // IMasterMessageBroker - Single master broker that holds the defined subscribers and message processing pipelines
        services.TryAddSingleton(svp =>
        {
            var mbb = svp.GetRequiredService<MessageBrokerBuilder>();
            var messageBrokerSettings = svp.GetRequiredService<MessageBrokerSettings>();

            //// Set the MessageBroker.Current
            //var currentBrokerProvider = svp.GetRequiredService<ICurrentMessageBrokerProvider>();
            //MessageBroker.SetProvider(currentBrokerProvider.GetCurrent);

            return mbb.Build();
        });

        services.TryAddTransient<ISubscriberControl>(svp => svp.GetRequiredService<IMessageBroker>());

        // Register transient message broker - this is a lightweight proxy that just introduces the current DI scope
        //services.TryAddTransient(svp => new MessageBrokerProxy(svp.GetRequiredService<IMessageBroker>(), svp));

        //services.TryAddTransient<IMessageBroker>(svp => svp.GetRequiredService<MessageBrokerProxy>());
        //services.TryAddTransient<IPublisher>(svp => svp.GetRequiredService<MessageBrokerProxy>());

        //services.TryAddSingleton<ICurrentMessageBrokerProvider, CurrentMessageBrokerProvider>();
        //services.TryAddSingleton<IMessageTypeResolver, AssemblyQualifiedNameMessageTypeResolver>();
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IMessageBrokerSettingsPostProcessor, SubscriberMethodPostProcessor>());

        services.AddHostedService<MessageBrokerHostedService>();

        return services;
    }

    public static void RegisterAllTypes<T>(this IServiceCollection services, Assembly[] assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));
        foreach (var type in typesFromAssemblies)
            services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
    }

    public static MessageBrokerBuilder AddServicesFromAssembly(
        this MessageBrokerBuilder mbb,
        Assembly assembly,
        Func<Type, bool> filter = null,
        ServiceLifetime consumerLifetime = ServiceLifetime.Transient)
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
                services.Add(new ServiceDescriptor(typeof(ISubscriber), type, consumerLifetime)); 
                services.TryAdd(ServiceDescriptor.Describe(type, type, consumerLifetime));
            }
        });

        return mbb;
    }

    //public static MessageBrokerBuilder AddServicesFromAssembly1(
    //    this MessageBrokerBuilder mbb,
    //    Assembly assembly,
    //    Func<Type, bool> filter = null,
    //    ServiceLifetime consumerLifetime = ServiceLifetime.Transient)
    //{
    //    var scan = ReflectionDiscoveryScanner.From(assembly);
    //    var foundConsumerTypes = scan.GetConsumerTypes(filter);

    //    mbb.PostConfigurationActions.Add(services =>
    //    {
    //        foreach (var (foundType, interfaceTypes) in foundConsumerTypes.GroupBy(x => x.ConsumerType, x => x.InterfaceType).ToDictionary(x => x.Key, x => x))
    //        {
    //            // Register the consumer/handler type
    //            services.TryAdd(ServiceDescriptor.Describe(foundType, foundType, consumerLifetime));

    //            foreach (var interfaceType in interfaceTypes)
    //            {
    //                if (foundType.IsGenericType && !foundType.IsConstructedGenericType)
    //                {
    //                    // Skip open generic types
    //                    continue;
    //                }

    //                // Register the interface of the consumer / handler
    //                services.TryAdd(ServiceDescriptor.Describe(interfaceType, svp => svp.GetRequiredService(foundType), consumerLifetime));
    //            }
    //        }
    //    });
    //    return mbb;
    //}

    public static MessageBrokerBuilder AddServicesFromAssemblyContaining<T>(
        this MessageBrokerBuilder mbb,
        Func<Type, bool> filter = null,
        ServiceLifetime consumerLifetime = ServiceLifetime.Transient,
        ServiceLifetime interceptorLifetime = ServiceLifetime.Transient) =>
        mbb.AddServicesFromAssembly(typeof(T).Assembly, filter, consumerLifetime: consumerLifetime);


}