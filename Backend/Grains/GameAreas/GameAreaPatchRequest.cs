using Microsoft.AspNetCore.JsonPatch;

namespace Grains.GameAreas
{
    public class GameAreaPatchRequest
    {
        public string TimelineMessage {get; set;}
        public JsonPatchDocument<GameAreaState> AreaPatchOperations {get; set;}
    }
}