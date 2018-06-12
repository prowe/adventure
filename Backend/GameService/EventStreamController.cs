using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using GameAreas;

namespace GameService
{
    public class EventStreamController : Controller
    {
        private ILogger<EventStreamController> logger;

        public EventStreamController(ILogger<EventStreamController> logger)
        {
            this.logger = logger;
        }

        [Route("event-stream")]
        public async Task<IActionResult> Handle()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                System.Console.WriteLine("WS connected" + webSocket);
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
            var timer = new Timer(_ => {
                var e = new GameAreaMessageEvent {
                    Message = $"The clock ticks. The time is now _{DateTime.Now}_"
                };
                webSocket.SendObject(e);
                logger.LogInformation("Fired from ws controller side: " + e);
                
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