using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SidusAPI.Migrations
{
    public partial class MigrationName2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitOnPath",
                table: "ServerNodes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UnitOnPath",
                table: "ServerNodes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
