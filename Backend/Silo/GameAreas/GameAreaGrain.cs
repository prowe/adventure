using System;
using System.Threading.Tasks;
using Orleans;

namespace GameAreas
{
    public class GameAreaGrain : Grain, IGameAreaGrain
    {


        public override async Task OnActivateAsync()
        {
            System.Console.WriteLine("Activating");
            await base.OnActivateAsync();
            base.RegisterTimer(TickTock, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        }

        private Task TickTock(object arg)
        {
            var message = $"The clock ticks. The time is now _{DateTime.Now}_";
            System.Console.WriteLine(message);
            return Task.CompletedTask;
        }

        public Task Initialize()
        {
            System.Console.WriteLine("Initializing");
            return Task.CompletedTask;
        }
    }
}