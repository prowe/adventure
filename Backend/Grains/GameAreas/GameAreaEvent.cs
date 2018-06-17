using Grains;

namespace Grains.GameAreas
{
    public class GameAreaEvent
    {
        public string TimelineMessage {get; set;}

        public override string ToString()
        {
            return $"GameAreaEvent: {TimelineMessage}";
        }
    }
}