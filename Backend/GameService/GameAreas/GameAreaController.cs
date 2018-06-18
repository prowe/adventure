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

namespace GameService.GameAreas
{
    public class GameAreaController : Controller
    {
        private readonly ILogger<GameAreaController> logger;
        private readonly IClusterClient clusterClient;

        public GameAreaController(ILogger<GameAreaController> logger, IClusterClient clusterClient)
        {
            this.logger = logger;
            this.clusterClient = clusterClient;
        }

        [Route("/game-areas/{id}")]
        [HttpPatch]
        public async Task<IActionResult> PatchArea([FromRoute]Guid id, [FromBody]GameAreaPatchRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            logger.LogInformation("Paching Area {}: {}", id, request);
            id = Guid.Empty; //FIXME
            IGameAreaGrain area = clusterClient.GetGrain<IGameAreaGrain>(id);
            var resultingState = await area.PatchArea(request);
            return Ok(resultingState);
        }
    }
}