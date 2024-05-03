namespace StartaneousAPI.Models
{
    public class NewGame
    {
        public Guid ClientId { get; set; }
        public Guid GameId { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }
    }
}
