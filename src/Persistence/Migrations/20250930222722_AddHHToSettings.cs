using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddHHToSettings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "happy_hours",
            table: "settings",
            type: "text",
            nullable: false,
            defaultValue: "Eu|19:00|23:00|Europe/Paris,Na|19:30|23:30|America/Chicago,As|20:00|23:59|Asia/Shanghai,Oc|20:00|23:59|Australia/Sydney");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "happy_hours",
            table: "settings");
    }
}
