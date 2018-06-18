using Grains;
using Microsoft.AspNetCore.JsonPatch;

namespace Grains.GameAreas
{
    public class GameAreaEvent
    {
        public string TimelineMessage {get; set;}
        public JsonPatchDocument<GameAreaState> AreaPatchOperations {get; set;}

        public override string ToString()
        {
            return $"GameAreaEvent: {TimelineMessage}";
        }
    }
}