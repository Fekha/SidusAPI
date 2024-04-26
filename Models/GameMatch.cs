using StarTaneousAPI.Models;

namespace StartaneousAPI.Models
{
    public class GameMatch
    {
        private int maxPlayers = 2;
        public Guid GameId { get; set; }
        public List<GameTurn> GameTurns { get; set; }
        public Player[] Players { get; set; }

        public GameMatch()
        {
            GameId = Guid.NewGuid();
            Players = new Player[maxPlayers];
            GameTurns = new List<GameTurn>();
        }
    }
}
