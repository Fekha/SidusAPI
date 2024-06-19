using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SidusAPI.Migrations
{
    public partial class MigrationName1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_GameTurns_GameGuid_TurnNumber",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerActions_Players_GameGuid_TurnNumber_PlayerId",
                table: "ServerActions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerTechnologies_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerTechnologies");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerUnits_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Players",
                table: "Players");

            migrationBuilder.RenameTable(
                name: "Players",
                newName: "GamePlayers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamePlayers",
                table: "GamePlayers",
                columns: new[] { "GameGuid", "TurnNumber", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_GameTurns_GameGuid_TurnNumber",
                table: "GamePlayers",
                columns: new[] { "GameGuid", "TurnNumber" },
                principalTable: "GameTurns",
                principalColumns: new[] { "GameGuid", "TurnNumber" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerActions_GamePlayers_GameGuid_TurnNumber_PlayerId",
                table: "ServerActions",
                columns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                principalTable: "GamePlayers",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerTechnologies_GamePlayers_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerTechnologies",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" },
                principalTable: "GamePlayers",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUnits_GamePlayers_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" },
                principalTable: "GamePlayers",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_GameTurns_GameGuid_TurnNumber",
                table: "GamePlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerActions_GamePlayers_GameGuid_TurnNumber_PlayerId",
                table: "ServerActions");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerTechnologies_GamePlayers_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerTechnologies");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerUnits_GamePlayers_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePlayers",
                table: "GamePlayers");

            migrationBuilder.RenameTable(
                name: "GamePlayers",
                newName: "Players");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Players",
                table: "Players",
                columns: new[] { "GameGuid", "TurnNumber", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GameTurns_GameGuid_TurnNumber",
                table: "Players",
                columns: new[] { "GameGuid", "TurnNumber" },
                principalTable: "GameTurns",
                principalColumns: new[] { "GameGuid", "TurnNumber" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerActions_Players_GameGuid_TurnNumber_PlayerId",
                table: "ServerActions",
                columns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                principalTable: "Players",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServerTechnologies_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerTechnologies",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" },
                principalTable: "Players",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUnits_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" },
                principalTable: "Players",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" });
        }
    }
}
