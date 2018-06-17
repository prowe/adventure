using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using Grains;
using Grains.GameAreas;
using Orleans;
using Orleans.Streams;

namespace GameService
{
    public class EventStreamController : Controller, IAsyncObserver<IGameAreaEvent>
    {
        private readonly ILogger<EventStreamController> logger;
        private readonly IClusterClient clusterClient;
        private WebSocket webSocket;
        private IAsyncStream<IGameAreaEvent> stream;


        public EventStreamController(ILogger<EventStreamController> logger, IClusterClient clusterClient)
        {
            this.logger = logger;
            this.clusterClient = clusterClient;
        }

        [Route("event-stream")]
        public async Task<IActionResult> Handle()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                logger.LogWarning("Got non WS request");
                return BadRequest();
            }

            webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            logger.LogInformation("WS connected" + webSocket);
            var stream = clusterClient.GetStreamProvider("SMSProvider")
                .GetStream<IGameAreaEvent>(Guid.Empty, null);
            await stream.SubscribeAsync(this);

            await WaitForWebSocketMessages();
            return Ok();
        }
        private async Task WaitForWebSocketMessages()
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            logger.LogInformation("Clsoing WS");
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(IGameAreaEvent gameEvent, StreamSequenceToken token = null)
        {
            logger.LogInformation("forwarding event " + gameEvent);
            try {
                await webSocket.SendObject(gameEvent);
            } catch (Exception e) {
                logger.LogWarning("Error on stream", e);
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occured on send", CancellationToken.None);
            }
        }
    }
}