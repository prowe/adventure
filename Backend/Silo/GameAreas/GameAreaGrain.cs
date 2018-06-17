using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Grains;
using Grains.GameAreas;
using Orleans.Streams;

namespace Silo.GameAreas
{
    public class GameAreaGrain : Grain, IGameAreaGrain
    {
        private readonly ILogger<GameAreaGrain> logger;
        private IAsyncStream<IGameAreaEvent> areaEventStream;

        public GameAreaGrain(ILogger<GameAreaGrain> logger)
        {
            this.logger = logger;
        }
        public override async Task OnActivateAsync()
        {
            logger.LogInformation("Activating");
            await base.OnActivateAsync();
            base.RegisterTimer(TickTock, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            areaEventStream = GetStreamProvider("SMSProvider")
                .GetStream<IGameAreaEvent>(GrainReference.GetPrimaryKey(), null);
        }

        private async Task TickTock(object arg)
        {
            var e = new GameAreaMessageEvent {
                PlayerTimelineMessage = $"The clock ticks. The time is now _{DateTime.Now}_"
            };
            logger.LogInformation("Sending event: {e}", e);
            await areaEventStream.OnNextAsync(e);
        }

        public Task Initialize()
        {
            logger.LogInformation("Initializing");
            return Task.CompletedTask;
        }
    }
}