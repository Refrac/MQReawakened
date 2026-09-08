using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Apps.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<short>(type: "INTEGER", nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScoreType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopScores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_GameId_CharacterId_ScoreType",
                table: "TopScores",
                columns: new[] { "GameId", "CharacterId", "ScoreType" });

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_GameId_ScoreType_Score",
                table: "TopScores",
                columns: new[] { "GameId", "ScoreType", "Score" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopScores");
        }
    }
}
