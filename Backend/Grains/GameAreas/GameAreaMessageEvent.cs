using Grains;

namespace Grains.GameAreas
{
    public class GameAreaMessageEvent : IGameEvent
    {
        public string PlayerTimelineMessage {get; set;}

        public override string ToString()
        {
            return $"GameAreaMessageEvent: {PlayerTimelineMessage}";
        }
    }
}