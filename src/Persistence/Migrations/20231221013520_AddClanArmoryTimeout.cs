using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddClanArmoryTimeout : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<TimeSpan>(
            name: "armory_timeout",
            table: "clans",
            type: "interval",
            nullable: false,
            defaultValue: new TimeSpan(3, 0, 0, 0, 0));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "armory_timeout",
            table: "clans");
    }
}
