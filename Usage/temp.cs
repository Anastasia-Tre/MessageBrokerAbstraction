//namespace Sample.Serialization.ConsoleApp;

//using global::Serialization.Json;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//using Sample.Serialization.MessagesAvro;

//using SecretStore;

//using SlimMessageBus;
//using SlimMessageBus.Host;
//using SlimMessageBus.Host.Memory;
//using SlimMessageBus.Host.Redis;
//using SlimMessageBus.Host.Serialization;
//using SlimMessageBus.Host.Serialization.Avro;
//using SlimMessageBus.Host.Serialization.Hybrid;
//using SlimMessageBus.Host.Serialization.Json;

//enum Provider
//{
//    Redis
//}

//class Program
//{
//    static async Task Main(string[] args) => await Host.CreateDefaultBuilder(args)
//        .ConfigureServices((ctx, services) => {
//            // Local file with secrets
//            Secrets.Load(@"..\..\..\..\..\secrets.txt");

//            services.AddHostedService<MainProgram>();

//            // alternatively a simpler approach, but using the slower ReflectionMessageCreationStategy and ReflectionSchemaLookupStrategy
//            var avroSerializer = new AvroMessageSerializer();

//            // Avro serialized using the AvroConvert library - no schema generation neeeded upfront.
//            var jsonSerializer = new JsonMessageSerializer();

//            services
//                .AddSlimMessageBus(mbb => {
//                    // Note: remember that Memory provider does not support req-resp yet.
//                    var provider = Provider.Redis;

//                    mbb
//                        .AddServicesFromAssemblyContaining<AddCommandSubscriber>()
//                        // Note: Certain messages will be serialized by one Avro serializer, other using the Json serializer
//                        .AddHybridSerializer(new Dictionary<IMessageSerializer, Type[]>
//                        {
//                            [jsonSerializer] = new[] { typeof(SubtractCommand) }, // the first one will be the default serializer, no need to declare types here
//                            [avroSerializer] = new[] { typeof(AddCommand), typeof(MultiplyRequest), typeof(MultiplyResponse) },
//                        }, defaultMessageSerializer: jsonSerializer)

//                        .Produce<AddCommand>(x => x.DefaultTopic("AddCommand"))
//                        .Consume<AddCommand>(x => x.Topic("AddCommand").WithSubscriber<AddCommandSubscriber>())

//                        .Produce<SubtractCommand>(x => x.DefaultTopic("SubtractCommand"))
//                        .Consume<SubtractCommand>(x => x.Topic("SubtractCommand").WithSubscriber<SubtractCommandSubscriber>())

//                        .Produce<MultiplyRequest>(x => x.DefaultTopic("MultiplyRequest"))
//                        .Handle<MultiplyRequest, MultiplyResponse>(x => x.Topic("MultiplyRequest").WithHandler<MultiplyRequestHandler>())

//                        .ExpectRequestResponses(x => x.ReplyToTopic("ConsoleApp"))

//                        .Do(builder => {
//                            Console.WriteLine($"Using {provider} as the transport provider");
//                            switch (provider)
//                            {
//                                case Provider.Redis:
//                                    builder.WithProviderRedis(cfg => cfg.ConnectionString = Secrets.Service.PopulateSecrets(ctx.Configuration["Redis:ConnectionString"]));
//                                    break;

//                                default:
//                                    throw new NotSupportedException();
//                            }
//                        });
//                });
//        })
//        .Build()
//        .RunAsync();
//}

//public class MainProgram : IHostedService
//{
//    private readonly IMessageBus _bus;
//    private readonly Random _random = new();
//    private volatile bool _canRun = true;
//    private Task _task;

//    public MainProgram(IMessageBus bus) => _bus = bus;

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        var addTask = Task.Factory.StartNew(AddLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
//        _task = Task.WhenAll(addTask);
//        return Task.CompletedTask;
//    }

//    public async Task StopAsync(CancellationToken cancellationToken)
//    {
//        _canRun = false;
//        await _task;
//    }

//    protected async Task AddLoop()
//    {
//        while (_canRun)
//        {
//            var a = _random.Next(100);
//            var b = _random.Next(100);
//            var opId = Guid.NewGuid().ToString();

//            Console.WriteLine("Publisher: Sending numbers {0} and {1}", a, b);
//            try
//            {
//                await _bus.Publish(new AddCommand { OperationId = opId, Left = a, Right = b });
//                await _bus.Publish(new SubtractCommand { OperationId = opId, Left = a, Right = b });
//            } catch (Exception e)
//            {
//                Console.WriteLine("Publisher: publish error {0}", e);
//            }

//            await Task.Delay(50); // Simulate some delay
//        }
//    }
//}

//public class AddCommandSubscriber : ISubscriber<AddCommand>
//{
//    public async Task OnHandle(AddCommand message)
//    {
//        Console.WriteLine("Subscriber: Adding {0} and {1} gives {2}", message.Left, message.Right, message.Left + message.Right);
//        await Task.Delay(50); // Simulate some work
//    }
//}

//public class SubtractCommandSubscriber : ISubscriber<SubtractCommand>
//{
//    public async Task OnHandle(SubtractCommand message)
//    {
//        Console.WriteLine("Subscriber: Subracting {0} and {1} gives {2}", message.Left, message.Right, message.Left - message.Right);
//        await Task.Delay(50); // Simulate some work
//    }
//}

///// <summary>
///// This will be serialized as JSON.
///// </summary>
//public class SubtractCommand
//{
//    public string OperationId { get; set; }
//    public int Left { get; set; }
//    public int Right { get; set; }
//}