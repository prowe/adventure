using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Grains.GameAreas
{
    public class GameAreaPatchRequest
    {
        public string TimelineMessage {get; set;}
        public JsonPatchDocument<GameAreaState> AreaPatchOperations {get; set;}

        public override string ToString()
        {
            var ops = AreaPatchOperations != null ? AreaPatchOperations.Operations : new List<Operation<GameAreaState>>();
            return $"Game Area Patch Request: {TimelineMessage} {ops}";
        }
    }
}