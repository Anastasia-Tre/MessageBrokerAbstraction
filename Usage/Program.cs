using MessageBroker.Core.MessageBroker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Provider.Redis;
using Serialization.Json;

namespace Usage;

internal class Program
{
    static async Task Main(string[] args) => await Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) =>
        {
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
                        Secrets.Service.PopulateSecrets(ctx.Configuration["Redis:ConnectionString"]);
                    var redisConnectionString2 =
                        Secrets.Service.PopulateSecrets("Redis:ConnectionString");
                    var redisConnectionString = "localhost:6379";

                    mbb.AddJsonSerializer()
                        .WithProviderRedis(cfg =>
                            cfg.ConnectionString = redisConnectionString);


                });
        })
        .Build()
        .RunAsync();
}