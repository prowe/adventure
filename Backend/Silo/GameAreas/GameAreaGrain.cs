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
        private IAsyncStream<GameAreaMessageEvent> areaStreamProvider;

        public GameAreaGrain(ILogger<GameAreaGrain> logger)
        {
            this.logger = logger;
        }
        public override async Task OnActivateAsync()
        {
            System.Console.WriteLine("Activating");
            await base.OnActivateAsync();
            base.RegisterTimer(TickTock, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            areaStreamProvider = GetStreamProvider("SMSProvider").GetStream<GameAreaMessageEvent>(GrainReference.GetPrimaryKey(), null);
        }

        private async Task TickTock(object arg)
        {
            var e = new GameAreaMessageEvent {
                PlayerTimelineMessage = $"The clock ticks. The time is now _{DateTime.Now}_"
            };
            logger.LogInformation("Sending event: {e}", e);
            await areaStreamProvider.OnNextAsync(e);
        }

        public Task Initialize()
        {
            System.Console.WriteLine("Initializing");
            return Task.CompletedTask;
        }
    }
}