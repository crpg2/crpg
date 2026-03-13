using System;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserItemPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_item_presets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_item_presets", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_item_presets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_item_preset_slots",
                columns: table => new
                {
                    user_item_preset_id = table.Column<int>(type: "integer", nullable: false),
                    slot = table.Column<ItemSlot>(type: "item_slot", nullable: false),
                    item_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_item_preset_slots", x => new { x.user_item_preset_id, x.slot });
                    table.ForeignKey(
                        name: "fk_user_item_preset_slots_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_item_preset_slots_user_item_presets_user_item_preset_id",
                        column: x => x.user_item_preset_id,
                        principalTable: "user_item_presets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_item_preset_slots_item_id",
                table: "user_item_preset_slots",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_item_presets_user_id",
                table: "user_item_presets",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_item_preset_slots");

            migrationBuilder.DropTable(
                name: "user_item_presets");
        }
    }
}
