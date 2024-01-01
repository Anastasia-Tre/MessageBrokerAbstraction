using Microsoft.Extensions.DependencyInjection;
using MessageBroker.Core.Publisher;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageBroker.Core.MessageBroker;

public static class MessageBrokerExtensions
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

            return (IMessageBroker)mbb.Build();
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

}