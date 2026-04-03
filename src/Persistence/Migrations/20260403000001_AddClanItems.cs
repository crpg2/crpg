using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddClanItems : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "clan_items",
            columns: table => new
            {
                user_item_id = table.Column<int>(type: "integer", nullable: false),
                clan_id = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_clan_items", x => x.user_item_id);

                table.ForeignKey(
                    name: "fk_clan_items_user_items_user_item_id",
                    column: x => x.user_item_id,
                    principalTable: "user_items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);

                table.ForeignKey(
                    name: "fk_clan_items_clans_clan_id",
                    column: x => x.clan_id,
                    principalTable: "clans",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_clan_items_clan_id",
            table: "clan_items",
            column: "clan_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "clan_items");
    }
}
