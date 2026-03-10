using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class SeparateGamemodeRating : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<float>(
            name: "competitive_rating",
            table: "character_statistics",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating",
            table: "character_statistics",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating_deviation",
            table: "character_statistics",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating_volatility",
            table: "character_statistics",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.Sql($@"
                UPDATE character_statistics
                SET
                    rating = c.rating,
                    rating_deviation = c.rating_deviation,
                    rating_volatility = c.rating_volatility,
                    competitive_rating = c.competitive_rating
                FROM characters c
                WHERE character_statistics.character_id = c.id
                AND character_statistics.game_mode = 'crpg_battle'");

        migrationBuilder.DropColumn(
            name: "competitive_rating",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "rating",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "rating_deviation",
            table: "characters");

        migrationBuilder.DropColumn(
            name: "rating_volatility",
            table: "characters");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<float>(
            name: "competitive_rating",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating_deviation",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.AddColumn<float>(
            name: "rating_volatility",
            table: "characters",
            type: "real",
            nullable: false,
            defaultValue: 0f);

        migrationBuilder.Sql($@"
                UPDATE characters
                SET
                    rating = cs.rating,
                    rating_deviation = cs.rating_deviation,
                    rating_volatility = cs.rating_volatility,
                    competitive_rating = cs.competitive_rating
                FROM character_statistics cs
                WHERE characters.id = cs.character_id
                AND cs.game_mode = 'crpg_battle'");

        migrationBuilder.DropColumn(
            name: "competitive_rating",
            table: "character_statistics");

        migrationBuilder.DropColumn(
            name: "rating",
            table: "character_statistics");

        migrationBuilder.DropColumn(
            name: "rating_deviation",
            table: "character_statistics");

        migrationBuilder.DropColumn(
            name: "rating_volatility",
            table: "character_statistics");
    }
}
