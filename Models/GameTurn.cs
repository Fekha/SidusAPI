namespace StartaneousAPI.Models
{
    public class GameTurn
    {
        public int TurnNumber { get; set; }
        public Turn[] ClientActions { get; set; }

        public GameTurn(int turnNumber, Turn clientActions, int pos)
        {
            TurnNumber = turnNumber;
            ClientActions = new Turn[2];
            ClientActions[pos] = clientActions;
        }
    }
}