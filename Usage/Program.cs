using MessageBroker.Core.DI;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Provider.Redis;
using SecretStore;
using Serialization.Json;

namespace Usage;

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
                        //// Load configuration
                        //var configuration = new ConfigurationBuilder()
                        //    .AddJsonFile("./../../../appsettings.json")
                        //    .Build();

                        Secrets.Load(@"..\..\..\..\..\secrets.txt");

                        var temp = ctx.Configuration["Redis:ConnectionString"];
                        var redisConnectionString1 =
                            Secrets.Service?.PopulateSecrets(
                                ctx.Configuration["Redis:ConnectionString"]);
                        var redisConnectionString2 =
                            Secrets.Service?.PopulateSecrets(
                                "Redis:ConnectionString");
                        var redisConnectionString = "localhost:6379";

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

public class MainProgram : IHostedService
{
    private readonly IMessageBroker _broker;
    private readonly Random _random = new();
    private volatile bool _canRun = true;
    private Task _task;

    public MainProgram(IMessageBroker broker)
    {
        _broker = broker;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var addTask = Task.Factory.StartNew(AddLoop, CancellationToken.None,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _task = Task.WhenAll(addTask);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _canRun = false;
        await _task.ConfigureAwait(false);
    }

    protected async Task AddLoop()
    {
        while (_canRun)
        {
            var a = _random.Next(100);
            var b = _random.Next(100);
            var opId = Guid.NewGuid().ToString();

            Console.WriteLine($"Publisher: Sending numbers {a} and {b}");
            try
            {
                await _broker.Publish(new AddCommand
                        { OperationId = opId, Left = a, Right = b })
                    .ConfigureAwait(false);
                await _broker.Publish(new SubtractCommand
                        { OperationId = opId, Left = a, Right = b })
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Publisher: publish error {e}");
            }

            await Task.Delay(1000).ConfigureAwait(false); // Simulate some work
        }
    }
}

public class AddCommandSubscriber : ISubscriber<AddCommand>
{
    public async Task OnHandle(AddCommand message)
    {
        Console.WriteLine(
            $"Subscriber: Adding {message.Left} and {message.Right} gives {message.Left + message.Right}");
        await Task.Delay(500).ConfigureAwait(false); // Simulate some work
    }
}

public class SubtractCommandSubscriber : ISubscriber<SubtractCommand>
{
    public async Task OnHandle(SubtractCommand message)
    {
        Console.WriteLine(
            $"Subscriber: Subracting {message.Left} and {message.Right} gives {message.Left - message.Right}");
        await Task.Delay(500).ConfigureAwait(false); // Simulate some work
    }
}

public class AddCommand
{
    public string? OperationId { get; set; }
    public int Left { get; set; }
    public int Right { get; set; }
}

public class SubtractCommand
{
    public string? OperationId { get; set; }
    public int Left { get; set; }
    public int Right { get; set; }
}
