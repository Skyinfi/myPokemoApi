using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyPokemoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPokemonRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaughtPokemonIds",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserPokemons",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PokemonId = table.Column<int>(type: "integer", nullable: false),
                    CaughtAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Nickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Experience = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExperienceToNextLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    Health = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    MaxHealth = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    BattlesWon = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BattlesLost = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastBattleAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPokemons", x => new { x.UserId, x.PokemonId });
                    table.ForeignKey(
                        name: "FK_UserPokemons_Pokemons_PokemonId",
                        column: x => x.PokemonId,
                        principalTable: "Pokemons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPokemons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPokemons_CaughtAt",
                table: "UserPokemons",
                column: "CaughtAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserPokemons_LastBattleAt",
                table: "UserPokemons",
                column: "LastBattleAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserPokemons_Level",
                table: "UserPokemons",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_UserPokemons_PokemonId",
                table: "UserPokemons",
                column: "PokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPokemons_UserId",
                table: "UserPokemons",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPokemons");

            migrationBuilder.AddColumn<string>(
                name: "CaughtPokemonIds",
                table: "Users",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
