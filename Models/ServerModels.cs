using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SidusAPI.ServerModels
{
    [Serializable]
    public class Account
    {
        [Key]
        public Guid PlayerGuid { get; set; }
        public string AccountId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public double? Rating { get; set; }
        public int? Wins { get; set; }
    }

    [Serializable]
    public class GameMatch
    {
        [Key]
        public Guid GameGuid { get; set; }
        public int MaxPlayers { get; set; }
        public int NumberOfModules { get; set; }
        public string? GameSettings { get; set; }
        public Guid Winner { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime HealthCheck { get; set; }
        public List<GameTurn>? GameTurns { get; set; }
    }

    [Serializable]
    public class GameTurn
    {
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public int TurnNumber { get; set; }
        public string? ModulesForMarket { get; set; }
        public string? MarketModuleGuids { get; set; }
        public bool TurnIsOver { get; set; }
        public List<GamePlayer>? Players { get; set; }
        public List<ServerModule>? AllModules { get; set; }
        public List<ServerNode>? AllNodes { get; set; }
    }

    [Serializable]
    public class ServerNode
    {
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public int TurnNumber { get; set; }
        [Key, Column(Order = 2)]
        public int X { get; set; }
        [Key, Column(Order = 3)]
        public int Y { get; set; }
        public bool IsRift { get; set; }
        public int MaxCredits { get; set; }
        public int Minerals { get; set; }
        public int CreditRegin { get; set; }
        public Guid OwnedByGuid { get; set; }
    }

    [Serializable]
    public class GamePlayer
    {
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public Guid PlayerGuid { get; set; }
        [Key, Column(Order = 2)]
        public string? PlayerName { get; set; }
        public int TurnNumber { get; set; }
        public int PlayerColor { get; set; }
        public string? ModulesGuids { get; set; }
        public int Credits { get; set; }
        public int MaxActions { get; set; }
        public int FleetCount { get; set; }
        public int BonusKinetic { get; set; }
        public int BonusThermal { get; set; }
        public int BonusExplosive { get; set; }
        public int BonusHP { get; set; }
        public int BonusMining { get; set; }
        public int Score { get; set; }
        public List<ServerUnit>? Units { get; set; }
        public List<ServerAction>? Actions { get; set; }
        public List<ServerTechnology>? Technology { get; set; }
    }

    [Serializable]
    public class ServerTechnology
    {
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public int TurnNumber { get; set; }
        [Key, Column(Order = 2)]
        public Guid PlayerGuid { get; set; }
        [Key, Column(Order = 3)]
        public int TechnologyId { get; set; }
        public int Level { get; set; }
        public int CurrentAmount { get; set; }
        public int NeededAmount { get; set; }
        public string? EffectText { get; set; }
        public string? CurrentEffectText { get; set; }
        public string? RequirementText { get; set; }
    }

    [Serializable]
    public class ServerAction
    {
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public int TurnNumber { get; set; }
        [Key, Column(Order = 2)]
        public Guid PlayerGuid { get; set; }
        [Key, Column(Order = 3)]
        public int ActionOrder { get; set; }
        public int? ActionTypeId { get; set; }
        public Guid? SelectedUnitGuid { get; set; }
        public string? XList { get; set; }
        public string? YList { get; set; }
        public Guid? SelectedModuleGuid { get; set; }
        public int? PlayerBid { get; set; }
        public Guid? GeneratedGuid { get; set; }
    }

    [Serializable]
    public class ServerUnit
    {
        public int UnitType { get; set; }
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public int TurnNumber { get; set; }
        [Key, Column(Order = 2)]
        public Guid PlayerGuid { get; set; }
        [Key, Column(Order = 3)]
        public Guid UnitGuid { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int Facing { get; set; }
        public string? UnitName { get; set; }
        public int PlayerColor { get; set; }
        public int TeamId { get; set; }
        public int MaxHP { get; set; }
        public int HP { get; set; }
        public int MaxMovement { get; set; }
        public int MovementLeft { get; set; }
        public int DeployRange { get; set; }
        public int KineticPower { get; set; }
        public int ThermalPower { get; set; }
        public int ExplosivePower { get; set; }
        public int KineticDamageModifier { get; set; }
        public int ThermalDamageModifier { get; set; }
        public int ExplosiveDamageModifier { get; set; }
        public int KineticDeployPower { get; set; }
        public int ThermalDeployPower { get; set; }
        public int ExplosiveDeployPower { get; set; }
        public int MaxMining { get; set; }
        public int MiningLeft { get; set; }
        public double SupportValue { get; set; }
        public int Level { get; set; }
        public int GlobalCreditGain { get; set; }
        public int MaxAttachedModules { get; set; }
        public string? AttachedModules { get; set; }
        public string? ModuleEffects { get; set; }
    }

    [Serializable]
    public class ServerModule
    {
        [Key, Column(Order = 0)]
        public Guid GameGuid { get; set; }
        [Key, Column(Order = 1)]
        public int TurnNumber { get; set; }
        [Key, Column(Order = 2)]
        public Guid ModuleGuid { get; set; }
        public int ModuleId { get; set; }
        public int MidBid { get; set; }
        public int TurnsLeft { get; set; }
    }
    [Serializable]
    public class Settings
    {
        public int ClientVersion { get; set; }
    }
}