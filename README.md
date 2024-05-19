# MessageBrokerAbstraction

MessageBrokerAbstraction is a versatile and flexible message broker client facade designed for .NET applications. This library aims to simplify and unify the interaction with various message brokers, providing a seamless abstraction layer for developers.

## Features

- **Lightweight Core**: Minimalist design for essential functionalities.
- **Unified API**: Simple, friendly interface for various messaging systems.
- **Zero External Dependencies**: No additional external requirements.
- **Domain Model Integration**: Seamlessly integrates with domain models (e.g., Domain Events).
- **Plugin Architecture**: Easily switch messaging brokers using NuGet packages.
- **JSON Serialization**: Built-in support for message serialization.
- **Async/Await Support**: Handles messaging asynchronously.
- **Fluent Configuration**: Clean, expressive setup for broker connections.
- **Logging Integration**: Connects with different logging providers for visibility.

## Providers

- **Memory**
- **Redis**
- **MQTT**
- **RabbitMQ** (in development)
- **Kafka** (in development)

## Sample

Example of configuration.

```
internal class Program
{
    private static async Task Main(string[] args)
    {
        await Host
            .CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddHostedService<MainProgram>();
                services
                    .AddMessageBroker(mbb =>
                    {
                        Secrets.Load(@"..\..\..\..\..\secrets.txt");

                        var redisConnectionString =
                            Secrets.Service?.PopulateSecrets(
                                ctx.Configuration["Redis:ConnectionString"]);

                        mbb.AddServicesFromAssemblyContaining<AddCommand>()
                            .AddJsonSerializer()
                            .WithProviderRedis(cfg =>
                                cfg.ConnectionString = redisConnectionString)

                            .SetPublisher<AddCommand>(x =>
                                x.DefaultTopic("AddCommand"))
                            .SetSubscriber<AddCommand>(x =>
                                x.Topic("AddCommand")
                                    .WithSubscriber<AddCommandSubscriber>())

                            .SetPublisher<SubtractCommand>(x =>
                                x.DefaultTopic("SubtractCommand"))
                            .SetSubscriber<SubtractCommand>(x =>
                                x.Topic("SubtractCommand")
                                    .WithSubscriber<
                                        SubtractCommandSubscriber>())
                            ;
                    });
            })
            .Build()
            .RunAsync().ConfigureAwait(false);
    }
}
```
