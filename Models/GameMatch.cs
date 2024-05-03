using StarTaneousAPI.Models;

namespace StartaneousAPI.Models
{
    public class GameMatch
    {
        public int MaxPlayers {get; set; }
        public Guid GameId { get; set; }
        public List<GameTurn> GameTurns { get; set; }
        public Player[] Players { get; set; }

        public GameMatch(NewGame clientGame)
        {
            GameId = Guid.NewGuid();
            Players = new Player[clientGame.MaxPlayers];
            Players[0] = new Player(clientGame.ClientId);
            MaxPlayers = clientGame.MaxPlayers;
            GameTurns = new List<GameTurn>();
        }
    }
}
