using System;
using Orleans;
using Orleans.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using System.Threading.Tasks;

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
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();

            Console.WriteLine("Press Enter to terminate...");
            Console.ReadLine();

            await host.StopAsync();
        }
    }
}
