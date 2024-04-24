namespace StartaneousAPI.Models
{
    [Serializable]
    public class Turn
    {
        public Guid GameId { get; set; }
        public Guid ClientId { get; set; }
        public int TurnNumber { get; set; }
        public List<ActionIds>? Actions { get; set; }
    }
}