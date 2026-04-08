using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddRatingUpdatedAt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "rating_updated_at",
            table: "character_statistics",
            type: "timestamp with time zone",
            nullable: true);

        // Seed existing rows with the current timestamp so that the decay worker
        // starts tracking inactivity from the moment of this deployment rather than
        // skipping all pre-existing players forever.
        migrationBuilder.Sql("UPDATE character_statistics SET rating_updated_at = NOW()");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "rating_updated_at",
            table: "character_statistics");
    }
}
