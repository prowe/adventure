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
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace GameService
{
    public class EventStreamController : Controller, IAsyncObserver<GameAreaEvent>
    {
        private readonly ILogger<EventStreamController> logger;
        private readonly IClusterClient clusterClient;
        private WebSocket webSocket;
        private IAsyncStream<GameAreaEvent> stream;

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
                .GetStream<GameAreaEvent>(Guid.Empty, null);
            await stream.SubscribeAsync(this);
            await PublishInitialAreaState();
            await WaitForWebSocketMessages();
            return Ok();
        }

        private async Task PublishInitialAreaState()
        {
            IGameAreaGrain area = clusterClient.GetGrain<IGameAreaGrain>(Guid.Empty);
            var state = await area.GetAreaState();
            var patchOps = new JsonPatchDocument<GameAreaState>();
            patchOps.Replace(ex => ex, state);
            await webSocket.SendObject(new GameAreaEvent
            {
                TimelineMessage = @"Welcome! You are now **connected**",
                AreaPatchOperations = patchOps
            });
            logger.LogInformation("published initial state");
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
            logger.LogError(ex, "OnErrorAsync");
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(GameAreaEvent gameEvent, StreamSequenceToken token = null)
        {
            logger.LogInformation("forwarding event " + gameEvent);
            try {
                await webSocket.SendObject(gameEvent);
            } catch (Exception e) {
                logger.LogWarning(e, "Error on stream");
                await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occured on send", CancellationToken.None);
            }
        }
    }
}