namespace StartaneousAPI.Models
{
    [Serializable]
    public class GameMatch
    {
        public int MaxPlayers {get; set; }
        public Guid GameGuid { get; set; }
        public List<GameTurn>? GameTurns { get; set; }
        public List<string>? GameSettings { get; set; }
    }

    [Serializable]
    public class GameTurn
    {
        public int TurnNumber { get; set; }
        public Player[]? Players { get; set; }
    }

    [Serializable]
    public class Player
    {
        public Unit? Station { get; set; }
        public List<Unit>? Fleets { get; set; }
        public List<Actions>? Actions { get; set; }
        public List<Guid>? ModulesGuids { get; set; }
        public int Credits { get; set; }
    }

    [Serializable]
    public class Actions
    {
        public int? ActionTypeId { get; set; }
        public Guid? SelectedUnitGuid { get; set; }
        public List<Guid>? SelectedModulesGuids { get; set; }
        public List<Coords>? SelectedCoords { get; set; }
        public int? GeneratedModuleId { get; set; }
        public Guid? GeneratedGuid { get; set; }
    }
    
    [Serializable]
    public class Unit
    {
        public Guid? UnitId { get; set; }
        public Coords? Location { get; set; }
        public Direction Facing { get; set; }
    }

    [Serializable]
    public class Coords
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
