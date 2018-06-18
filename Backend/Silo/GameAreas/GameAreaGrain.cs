using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Grains;
using Grains.GameAreas;
using Orleans.Streams;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Serialization;

namespace Silo.GameAreas
{
    public class GameAreaGrain : Grain<GameAreaState>, IGameAreaGrain
    {
        private readonly ILogger<GameAreaGrain> logger;
        private IAsyncStream<GameAreaEvent> areaEventStream;

        public GameAreaGrain(ILogger<GameAreaGrain> logger)
        {
            this.logger = logger;
        }
        public override async Task OnActivateAsync()
        {
            logger.LogInformation("Activating");
            await base.OnActivateAsync();
            this.State = new GameAreaState();
            base.RegisterTimer(TickTock, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            areaEventStream = GetStreamProvider("SMSProvider")
                .GetStream<GameAreaEvent>(GrainReference.GetPrimaryKey(), null);
        }

        private async Task TickTock(object arg)
        {
            var e = new GameAreaEvent {
                TimelineMessage = $"The clock ticks. The time is now _{DateTime.Now}_"
            };
            logger.LogInformation("Sending event: {e}", e);
            await areaEventStream.OnNextAsync(e);
        }

        public Task Initialize()
        {
            logger.LogInformation("Initializing");
            return Task.CompletedTask;
        }

        public Task<GameAreaState> GetAreaState()
        {
            return Task.FromResult(State);
        }

        public async Task<GameAreaState> PatchArea(GameAreaPatchRequest patchRequest)
        {
            logger.LogInformation("Patching area {}: {} -> {}", IdentityString, State, patchRequest);
            PatchStateIfNeeded(patchRequest.AreaPatchOperations);
            var e = new GameAreaEvent {
                TimelineMessage = patchRequest.TimelineMessage
            };
            await areaEventStream.OnNextAsync(e);
            return State;
        }

        private void PatchStateIfNeeded(JsonPatchDocument<GameAreaState> patchDocument)
        {
            if(patchDocument == null)
            {
                return;
            }
            try 
            {
                patchDocument.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                patchDocument.ApplyTo(State, err => logger.LogWarning("Error patching document: {}", err));
            } 
            catch (Exception e)
            {
                logger.LogWarning(e, "Error patching");
                throw e;
            }
        }
    }
}