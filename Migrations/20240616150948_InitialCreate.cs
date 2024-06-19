using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SidusAPI.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameMatches",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    NumberOfModules = table.Column<int>(type: "int", nullable: false),
                    GameSettings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Winner = table.Column<int>(type: "int", nullable: false),
                    HealthCheck = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMatches", x => x.GameGuid);
                });

            migrationBuilder.CreateTable(
                name: "GameTurns",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    ModulesForMarket = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllModules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TurnIsOver = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTurns", x => new { x.GameGuid, x.TurnNumber });
                    table.ForeignKey(
                        name: "FK_GameTurns_GameMatches_GameGuid",
                        column: x => x.GameGuid,
                        principalTable: "GameMatches",
                        principalColumn: "GameGuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerModules",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    ModuleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    MidBid = table.Column<int>(type: "int", nullable: false),
                    PlayerBid = table.Column<int>(type: "int", nullable: false),
                    TurnsLeft = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerModules", x => new { x.GameGuid, x.TurnNumber, x.ModuleGuid });
                    table.ForeignKey(
                        name: "FK_ServerModules_GameTurns_GameGuid_TurnNumber",
                        columns: x => new { x.GameGuid, x.TurnNumber },
                        principalTable: "GameTurns",
                        principalColumns: new[] { "GameGuid", "TurnNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerNodes",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    UnitOnPath = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRift = table.Column<bool>(type: "bit", nullable: false),
                    MaxCredits = table.Column<int>(type: "int", nullable: false),
                    Minerals = table.Column<int>(type: "int", nullable: false),
                    CreditRegin = table.Column<int>(type: "int", nullable: false),
                    OwnedById = table.Column<int>(type: "int", nullable: false),
                    GameTurnGameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GameTurnTurnNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerNodes", x => new { x.GameGuid, x.TurnNumber, x.X, x.Y });
                    table.ForeignKey(
                        name: "FK_ServerNodes_GameTurns_GameTurnGameGuid_GameTurnTurnNumber",
                        columns: x => new { x.GameTurnGameGuid, x.GameTurnTurnNumber },
                        principalTable: "GameTurns",
                        principalColumns: new[] { "GameGuid", "TurnNumber" });
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    StationGameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StationTurnNumber = table.Column<int>(type: "int", nullable: true),
                    StationPlayerId = table.Column<int>(type: "int", nullable: true),
                    StationUnitGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModulesGuids = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    MaxActions = table.Column<int>(type: "int", nullable: false),
                    FleetCount = table.Column<int>(type: "int", nullable: false),
                    BonusKinetic = table.Column<int>(type: "int", nullable: false),
                    BonusThermal = table.Column<int>(type: "int", nullable: false),
                    BonusExplosive = table.Column<int>(type: "int", nullable: false),
                    BonusHP = table.Column<int>(type: "int", nullable: false),
                    BonusMining = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => new { x.GameGuid, x.TurnNumber, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_Players_GameTurns_GameGuid_TurnNumber",
                        columns: x => new { x.GameGuid, x.TurnNumber },
                        principalTable: "GameTurns",
                        principalColumns: new[] { "GameGuid", "TurnNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerActions",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    ActionOrder = table.Column<int>(type: "int", nullable: false),
                    ActionTypeId = table.Column<int>(type: "int", nullable: true),
                    SelectedUnitGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    X = table.Column<int>(type: "int", nullable: true),
                    Y = table.Column<int>(type: "int", nullable: true),
                    SelectedModuleGameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SelectedModuleTurnNumber = table.Column<int>(type: "int", nullable: true),
                    SelectedModuleModuleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GeneratedGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerActions", x => new { x.GameGuid, x.TurnNumber, x.PlayerId, x.ActionOrder });
                    table.ForeignKey(
                        name: "FK_ServerActions_Players_GameGuid_TurnNumber_PlayerId",
                        columns: x => new { x.GameGuid, x.TurnNumber, x.PlayerId },
                        principalTable: "Players",
                        principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServerActions_ServerModules_SelectedModuleGameGuid_SelectedModuleTurnNumber_SelectedModuleModuleGuid",
                        columns: x => new { x.SelectedModuleGameGuid, x.SelectedModuleTurnNumber, x.SelectedModuleModuleGuid },
                        principalTable: "ServerModules",
                        principalColumns: new[] { "GameGuid", "TurnNumber", "ModuleGuid" });
                });

            migrationBuilder.CreateTable(
                name: "ServerTechnologies",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TechnologyId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CurrentAmount = table.Column<int>(type: "int", nullable: false),
                    NeededAmount = table.Column<int>(type: "int", nullable: false),
                    EffectText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentEffectText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequirementText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GamePlayerGameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GamePlayerPlayerId = table.Column<int>(type: "int", nullable: true),
                    GamePlayerTurnNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerTechnologies", x => new { x.GameGuid, x.TurnNumber, x.PlayerId, x.TechnologyId });
                    table.ForeignKey(
                        name: "FK_ServerTechnologies_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                        columns: x => new { x.GamePlayerGameGuid, x.GamePlayerTurnNumber, x.GamePlayerPlayerId },
                        principalTable: "Players",
                        principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" });
                });

            migrationBuilder.CreateTable(
                name: "ServerUnits",
                columns: table => new
                {
                    GameGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    UnitGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    X = table.Column<int>(type: "int", nullable: true),
                    Y = table.Column<int>(type: "int", nullable: true),
                    Facing = table.Column<int>(type: "int", nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    MaxHP = table.Column<int>(type: "int", nullable: false),
                    HP = table.Column<int>(type: "int", nullable: false),
                    MaxMovement = table.Column<int>(type: "int", nullable: false),
                    MovementLeft = table.Column<int>(type: "int", nullable: false),
                    KineticPower = table.Column<int>(type: "int", nullable: false),
                    ThermalPower = table.Column<int>(type: "int", nullable: false),
                    ExplosivePower = table.Column<int>(type: "int", nullable: false),
                    KineticDamageModifier = table.Column<int>(type: "int", nullable: false),
                    ThermalDamageModifier = table.Column<int>(type: "int", nullable: false),
                    ExplosiveDamageModifier = table.Column<int>(type: "int", nullable: false),
                    MaxMining = table.Column<int>(type: "int", nullable: false),
                    MiningLeft = table.Column<int>(type: "int", nullable: false),
                    SupportValue = table.Column<double>(type: "float", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    GlobalCreditGain = table.Column<int>(type: "int", nullable: false),
                    MaxAttachedModules = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachedModules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModuleEffects = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerUnits", x => new { x.GameGuid, x.TurnNumber, x.PlayerId, x.UnitGuid });
                    table.ForeignKey(
                        name: "FK_ServerUnits_Players_GameGuid_TurnNumber_PlayerId",
                        columns: x => new { x.GameGuid, x.TurnNumber, x.PlayerId },
                        principalTable: "Players",
                        principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players",
                columns: new[] { "StationGameGuid", "StationTurnNumber", "StationPlayerId", "StationUnitGuid" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerActions_SelectedModuleGameGuid_SelectedModuleTurnNumber_SelectedModuleModuleGuid",
                table: "ServerActions",
                columns: new[] { "SelectedModuleGameGuid", "SelectedModuleTurnNumber", "SelectedModuleModuleGuid" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerNodes_GameTurnGameGuid_GameTurnTurnNumber",
                table: "ServerNodes",
                columns: new[] { "GameTurnGameGuid", "GameTurnTurnNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ServerTechnologies_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerTechnologies",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Players_ServerUnits_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players",
                columns: new[] { "StationGameGuid", "StationTurnNumber", "StationPlayerId", "StationUnitGuid" },
                principalTable: "ServerUnits",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId", "UnitGuid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTurns_GameMatches_GameGuid",
                table: "GameTurns");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameTurns_GameGuid_TurnNumber",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_ServerUnits_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players");

            migrationBuilder.DropTable(
                name: "ServerActions");

            migrationBuilder.DropTable(
                name: "ServerNodes");

            migrationBuilder.DropTable(
                name: "ServerTechnologies");

            migrationBuilder.DropTable(
                name: "ServerModules");

            migrationBuilder.DropTable(
                name: "GameMatches");

            migrationBuilder.DropTable(
                name: "GameTurns");

            migrationBuilder.DropTable(
                name: "ServerUnits");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
