using System;
using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Threading.Tasks;
using System.Threading;
using GameAreas;
using Microsoft.Extensions.DependencyInjection;

namespace Silo
{
    class Program
    {
        public static void Main(string[] args)
        {
            RunSilo().GetAwaiter().GetResult();
        }

        private static async Task RunSilo() {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MyAwesomeService";
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GameAreaGrain).Assembly)
                    .WithReferences()
                    .WithCodeGeneration())
                .ConfigureLogging(logging => logging.AddConsole())
                .AddSimpleMessageStreamProvider("SMSProvider")
                .AddMemoryGrainStorage("PubSubStore")
                .AddStartupTask(InitStartingRoom);

            var host = builder.Build();
            await host.StartAsync();

            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();

            await host.StopAsync();
        }

        private static async Task InitStartingRoom(IServiceProvider services, CancellationToken cancellation)
        {
            var grainFactory = services.GetRequiredService<IGrainFactory>();

            // Get a reference to a grain and call a method on it.
            var grain = grainFactory.GetGrain<IGameAreaGrain>(Guid.Empty);
            await grain.Initialize();
        }
    }
}
