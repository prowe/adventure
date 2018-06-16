using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading;
using GameAreas;
using System.Text;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace GameService
{
    public static class ClusterClientFactory
    {
        public static IClusterClient BuildClusterClient(IServiceProvider services)
        {
            var builder = new ClientBuilder()
                // Use localhost clustering for a single local silo
                .UseLocalhostClustering()
                // Configure ClusterId and ServiceId
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MyAwesomeService";
                })
                .AddSimpleMessageStreamProvider("SMSProvider")
                .ConfigureLogging(logging => logging.AddConsole());
            var client = builder.Build();
            client.Connect().GetAwaiter().GetResult();
            return client;
        }
    }
}