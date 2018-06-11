using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Threading;
using GameAreas;
using System.Text;

namespace GameService
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
           // services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();
            app.Use(WebSocketMiddleware);
        }

        public async Task WebSocketMiddleware(HttpContext context, Func<Task> next)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                System.Console.WriteLine("WS connected" + webSocket);
                await TickTock(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        public async Task TickTock(HttpContext context, WebSocket webSocket)
        {
            var timer = new Timer(_ => {
                var message = $"The clock ticks. The time is now _{DateTime.Now}_";
                var b = new ArraySegment<byte>(Encoding.Unicode.GetBytes(message));
                webSocket.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
                System.Console.WriteLine("Fired from ws side: " + message);
                
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                // await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            System.Console.WriteLine("Clsoing WS");
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
