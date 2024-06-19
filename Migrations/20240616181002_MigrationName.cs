using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SidusAPI.Migrations
{
    public partial class MigrationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_ServerUnits_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_ServerUnits_Players_GameGuid_TurnNumber_PlayerId",
                table: "ServerUnits");

            migrationBuilder.DropIndex(
                name: "IX_Players_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "X",
                table: "ServerActions");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "ServerActions");

            migrationBuilder.DropColumn(
                name: "StationGameGuid",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StationPlayerId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StationTurnNumber",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StationUnitGuid",
                table: "Players");

            migrationBuilder.AddColumn<Guid>(
                name: "GamePlayerGameGuid",
                table: "ServerUnits",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GamePlayerPlayerId",
                table: "ServerUnits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GamePlayerTurnNumber",
                table: "ServerUnits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStation",
                table: "ServerUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "XList",
                table: "ServerActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YList",
                table: "ServerActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlayerGuid",
                table: "Players",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServerUnits_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUnits_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits",
                columns: new[] { "GamePlayerGameGuid", "GamePlayerTurnNumber", "GamePlayerPlayerId" },
                principalTable: "Players",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerUnits_Players_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits");

            migrationBuilder.DropIndex(
                name: "IX_ServerUnits_GamePlayerGameGuid_GamePlayerTurnNumber_GamePlayerPlayerId",
                table: "ServerUnits");

            migrationBuilder.DropColumn(
                name: "GamePlayerGameGuid",
                table: "ServerUnits");

            migrationBuilder.DropColumn(
                name: "GamePlayerPlayerId",
                table: "ServerUnits");

            migrationBuilder.DropColumn(
                name: "GamePlayerTurnNumber",
                table: "ServerUnits");

            migrationBuilder.DropColumn(
                name: "IsStation",
                table: "ServerUnits");

            migrationBuilder.DropColumn(
                name: "XList",
                table: "ServerActions");

            migrationBuilder.DropColumn(
                name: "YList",
                table: "ServerActions");

            migrationBuilder.DropColumn(
                name: "PlayerGuid",
                table: "Players");

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "ServerActions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "ServerActions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StationGameGuid",
                table: "Players",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationPlayerId",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationTurnNumber",
                table: "Players",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StationUnitGuid",
                table: "Players",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players",
                columns: new[] { "StationGameGuid", "StationTurnNumber", "StationPlayerId", "StationUnitGuid" });

            migrationBuilder.AddForeignKey(
                name: "FK_Players_ServerUnits_StationGameGuid_StationTurnNumber_StationPlayerId_StationUnitGuid",
                table: "Players",
                columns: new[] { "StationGameGuid", "StationTurnNumber", "StationPlayerId", "StationUnitGuid" },
                principalTable: "ServerUnits",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId", "UnitGuid" });

            migrationBuilder.AddForeignKey(
                name: "FK_ServerUnits_Players_GameGuid_TurnNumber_PlayerId",
                table: "ServerUnits",
                columns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                principalTable: "Players",
                principalColumns: new[] { "GameGuid", "TurnNumber", "PlayerId" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
