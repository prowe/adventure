namespace GameAreas
{
    public class GameAreaMessageEvent
    {
        public string Message {get; set;}

        public override string ToString()
        {
            return $"GameAreaMessageEvent: {Message}";
        }
    }
}