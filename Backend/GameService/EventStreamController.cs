using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using GameAreas;
using Orleans;
using Orleans.Streams;

namespace GameService
{
    public class EventStreamController : Controller
    {
        private readonly ILogger<EventStreamController> logger;
        private readonly IClusterClient clusterClient;

        public EventStreamController(ILogger<EventStreamController> logger, IClusterClient clusterClient)
        {
            this.logger = logger;
            this.clusterClient = clusterClient;
        }

        [Route("event-stream")]
        public async Task<IActionResult> Handle()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                System.Console.WriteLine("WS connected" + webSocket);

                var stream = clusterClient.GetStreamProvider("SMSProvider")
                    .GetStream<GameAreaMessageEvent>(Guid.Empty, null);
                var bridgeObserver = new StreamToWebSocketBridge(webSocket);
                await stream.SubscribeAsync(bridgeObserver);

                await TickTock(webSocket);
                return Ok();
            }
            else
            {
                logger.LogWarning("Got non WS request");
                return BadRequest();
            }
        }
        private async Task TickTock(WebSocket webSocket)
        {
            // var timer = new Timer(_ => {
            //     var e = new GameAreaMessageEvent {
            //         Message = $"The clock ticks. The time is now _{DateTime.Now}_"
            //     };
            //     webSocket.SendObject(e);
            //     logger.LogInformation("Fired from ws controller side: " + e);
                
            // }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
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

    public class StreamToWebSocketBridge : IAsyncObserver<GameAreaMessageEvent>
    {
        private readonly WebSocket webSocket;

        public StreamToWebSocketBridge(WebSocket webSocket)
        {
            this.webSocket = webSocket;
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(GameAreaMessageEvent gameEvent, StreamSequenceToken token = null)
        {
            System.Console.WriteLine("forwarding event " + gameEvent);
            await webSocket.SendObject(gameEvent);
        }
    }
}