using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;

namespace GameAreas
{
    public class GameAreaGrain : Grain, IGameAreaGrain
    {
        private IAsyncStream<GameAreaMessageEvent> areaStreamProvider;

        public override async Task OnActivateAsync()
        {
            System.Console.WriteLine("Activating");
            await base.OnActivateAsync();
            base.RegisterTimer(TickTock, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            areaStreamProvider = GetStreamProvider("SMSProvider").GetStream<GameAreaMessageEvent>(Guid.Empty, null);
        }

        private async Task TickTock(object arg)
        {
            var message = $"The clock ticks. The time is now _{DateTime.Now}_";
            System.Console.WriteLine(message);
            await areaStreamProvider.OnNextAsync(new GameAreaMessageEvent {
                Message = message
            });
        }

        public Task Initialize()
        {
            System.Console.WriteLine("Initializing");
            return Task.CompletedTask;
        }
    }
}