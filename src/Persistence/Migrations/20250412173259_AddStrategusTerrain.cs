using System;
using Crpg.Domain.Entities.Terrains;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddStrategusTerrain : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:terrain_type", "barrier,deep_water,shallow_water,sparse_forest,thick_forest");

        migrationBuilder.CreateTable(
            name: "terrains",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<TerrainType>(type: "terrain_type", nullable: false),
                boundary = table.Column<Polygon>(type: "geometry", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_terrains", x => x.id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "terrains");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:terrain_type", "barrier,deep_water,shallow_water,sparse_forest,thick_forest");
    }
}
