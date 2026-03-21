using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class MergeItemStacks : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Create the new unified table
        migrationBuilder.CreateTable(
            name: "item_stacks",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                item_id = table.Column<string>(type: "text", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                party_id = table.Column<int>(type: "integer", nullable: true),
                settlement_id = table.Column<int>(type: "integer", nullable: true),
                party_transfer_offer_id = table.Column<int>(type: "integer", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_item_stacks", x => x.id);
                table.CheckConstraint("CK_item_stacks_single_owner", "(party_id IS NOT NULL)::int + (settlement_id IS NOT NULL)::int + (party_transfer_offer_id IS NOT NULL)::int = 1");
                table.ForeignKey(
                    name: "fk_item_stacks_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_stacks_parties_party_id",
                    column: x => x.party_id,
                    principalTable: "parties",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_stacks_party_transfer_offers_party_transfer_offer_id",
                    column: x => x.party_transfer_offer_id,
                    principalTable: "party_transfer_offers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_item_stacks_settlements_settlement_id",
                    column: x => x.settlement_id,
                    principalTable: "settlements",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_item_stacks_item_id",
            table: "item_stacks",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_item_stacks_party_id_item_id",
            table: "item_stacks",
            columns: new[] { "party_id", "item_id" },
            unique: true,
            filter: "party_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_item_stacks_party_transfer_offer_id_item_id",
            table: "item_stacks",
            columns: new[] { "party_transfer_offer_id", "item_id" },
            unique: true,
            filter: "party_transfer_offer_id IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_item_stacks_settlement_id_item_id",
            table: "item_stacks",
            columns: new[] { "settlement_id", "item_id" },
            unique: true,
            filter: "settlement_id IS NOT NULL");

        // 2. Migrate data from old tables
        migrationBuilder.Sql(@"
                INSERT INTO item_stacks (item_id, count, party_id, settlement_id, party_transfer_offer_id, updated_at, created_at)
                SELECT item_id, count, party_id, NULL, NULL, updated_at, created_at
                FROM party_items;
            ");

        migrationBuilder.Sql(@"
                INSERT INTO item_stacks (item_id, count, party_id, settlement_id, party_transfer_offer_id, updated_at, created_at)
                SELECT item_id, count, NULL, settlement_id, NULL, updated_at, created_at
                FROM settlement_items;
            ");

        migrationBuilder.Sql(@"
                INSERT INTO item_stacks (item_id, count, party_id, settlement_id, party_transfer_offer_id, updated_at, created_at)
                SELECT item_id, count, NULL, NULL, party_transfer_offer_id, now(), now()
                FROM party_transfer_offer_items;
            ");

        // 3. Drop old tables
        migrationBuilder.DropTable(
            name: "party_items");

        migrationBuilder.DropTable(
            name: "settlement_items");

        migrationBuilder.DropTable(
            name: "party_transfer_offer_items");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Recreate old tables
        migrationBuilder.CreateTable(
            name: "party_items",
            columns: table => new
            {
                party_id = table.Column<int>(type: "integer", nullable: false),
                item_id = table.Column<string>(type: "text", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_party_items", x => new { x.party_id, x.item_id });
                table.ForeignKey(
                    name: "fk_party_items_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_party_items_parties_party_id",
                    column: x => x.party_id,
                    principalTable: "parties",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "party_transfer_offer_items",
            columns: table => new
            {
                party_transfer_offer_id = table.Column<int>(type: "integer", nullable: false),
                item_id = table.Column<string>(type: "text", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_party_transfer_offer_items", x => new { x.party_transfer_offer_id, x.item_id });
                table.ForeignKey(
                    name: "fk_party_transfer_offer_items_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_party_transfer_offer_items_party_transfer_offers_party_tran",
                    column: x => x.party_transfer_offer_id,
                    principalTable: "party_transfer_offers",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "settlement_items",
            columns: table => new
            {
                settlement_id = table.Column<int>(type: "integer", nullable: false),
                item_id = table.Column<string>(type: "text", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_settlement_items", x => new { x.settlement_id, x.item_id });
                table.ForeignKey(
                    name: "fk_settlement_items_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_settlement_items_settlements_settlement_id",
                    column: x => x.settlement_id,
                    principalTable: "settlements",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Migrate data back
        migrationBuilder.Sql(@"
                INSERT INTO party_items (party_id, item_id, count, updated_at, created_at)
                SELECT party_id, item_id, count, updated_at, created_at
                FROM item_stacks WHERE party_id IS NOT NULL;
            ");

        migrationBuilder.Sql(@"
                INSERT INTO settlement_items (settlement_id, item_id, count, updated_at, created_at)
                SELECT settlement_id, item_id, count, updated_at, created_at
                FROM item_stacks WHERE settlement_id IS NOT NULL;
            ");

        migrationBuilder.Sql(@"
                INSERT INTO party_transfer_offer_items (party_transfer_offer_id, item_id, count)
                SELECT party_transfer_offer_id, item_id, count
                FROM item_stacks WHERE party_transfer_offer_id IS NOT NULL;
            ");

        migrationBuilder.CreateIndex(
            name: "ix_party_items_item_id",
            table: "party_items",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_transfer_offer_items_item_id",
            table: "party_transfer_offer_items",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_settlement_items_item_id",
            table: "settlement_items",
            column: "item_id");

        migrationBuilder.DropTable(
            name: "item_stacks");
    }
}
