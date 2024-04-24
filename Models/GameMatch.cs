namespace StartaneousAPI.Models
{
    public class GameMatch
    {
        private int maxPlayers = 2;
        public Guid GameId { get; set; }
        public List<GameTurn> GameTurns { get; set; }
        public Guid[] Clients { get; set; }

        public GameMatch()
        {
            GameId = Guid.NewGuid();
            Clients = new Guid[maxPlayers];
            GameTurns = new List<GameTurn>();
        }
    }
}
