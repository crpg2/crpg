using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPartyOrders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_parties_parties_targeted_party_id",
            table: "parties");

        migrationBuilder.DropForeignKey(
            name: "fk_parties_settlements_targeted_settlement_id",
            table: "parties");

        migrationBuilder.DropColumn(
            name: "waypoints",
            table: "parties");

        migrationBuilder.RenameColumn(
            name: "targeted_settlement_id",
            table: "parties",
            newName: "current_settlement_id");

        migrationBuilder.RenameColumn(
            name: "targeted_party_id",
            table: "parties",
            newName: "current_party_id");

        migrationBuilder.RenameIndex(
            name: "ix_parties_targeted_settlement_id",
            table: "parties",
            newName: "ix_parties_current_settlement_id");

        migrationBuilder.RenameIndex(
            name: "ix_parties_targeted_party_id",
            table: "parties",
            newName: "ix_parties_current_party_id");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:party_order_type", "attack_party,attack_settlement,follow_party,join_battle,move_to_point,move_to_settlement,transfer_offer_party")
            .Annotation("Npgsql:Enum:party_status", "awaiting_battle_join_decision,awaiting_party_offer_decision,idle,idle_in_settlement,in_battle,recruiting_in_settlement")
            .Annotation("Npgsql:Enum:party_transfer_offer_status", "intent,pending")
            .OldAnnotation("Npgsql:Enum:party_status", "following_party,idle,idle_in_settlement,in_battle,moving_to_attack_party,moving_to_attack_settlement,moving_to_point,moving_to_settlement,recruiting_in_settlement");

        migrationBuilder.AddColumn<int>(
            name: "current_battle_id",
            table: "parties",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "party_orders",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                party_id = table.Column<int>(type: "integer", nullable: false),
                type = table.Column<PartyOrderType>(type: "party_order_type", nullable: false),
                order_index = table.Column<int>(type: "integer", nullable: false),
                waypoints = table.Column<MultiPoint>(type: "geometry", nullable: false),
                targeted_party_id = table.Column<int>(type: "integer", nullable: true),
                targeted_settlement_id = table.Column<int>(type: "integer", nullable: true),
                targeted_battle_id = table.Column<int>(type: "integer", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_party_orders", x => x.id);
                table.ForeignKey(
                    name: "fk_party_orders_battles_targeted_battle_id",
                    column: x => x.targeted_battle_id,
                    principalTable: "battles",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_party_orders_parties_party_id",
                    column: x => x.party_id,
                    principalTable: "parties",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_party_orders_parties_targeted_party_id",
                    column: x => x.targeted_party_id,
                    principalTable: "parties",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_party_orders_settlements_targeted_settlement_id",
                    column: x => x.targeted_settlement_id,
                    principalTable: "settlements",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "party_transfer_offers",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                party_id = table.Column<int>(type: "integer", nullable: false),
                target_party_id = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<PartyTransferOfferStatus>(type: "party_transfer_offer_status", nullable: false),
                gold = table.Column<int>(type: "integer", nullable: false),
                troops = table.Column<float>(type: "real", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_party_transfer_offers", x => x.id);
                table.ForeignKey(
                    name: "fk_party_transfer_offers_parties_party_id",
                    column: x => x.party_id,
                    principalTable: "parties",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_party_transfer_offers_parties_target_party_id",
                    column: x => x.target_party_id,
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

        migrationBuilder.CreateIndex(
            name: "ix_parties_current_battle_id",
            table: "parties",
            column: "current_battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_orders_party_id",
            table: "party_orders",
            column: "party_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_orders_targeted_battle_id",
            table: "party_orders",
            column: "targeted_battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_orders_targeted_party_id",
            table: "party_orders",
            column: "targeted_party_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_orders_targeted_settlement_id",
            table: "party_orders",
            column: "targeted_settlement_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_transfer_offer_items_item_id",
            table: "party_transfer_offer_items",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_transfer_offers_party_id",
            table: "party_transfer_offers",
            column: "party_id");

        migrationBuilder.CreateIndex(
            name: "ix_party_transfer_offers_target_party_id",
            table: "party_transfer_offers",
            column: "target_party_id");

        migrationBuilder.AddForeignKey(
            name: "fk_parties_battles_current_battle_id",
            table: "parties",
            column: "current_battle_id",
            principalTable: "battles",
            principalColumn: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_parties_parties_current_party_id",
            table: "parties",
            column: "current_party_id",
            principalTable: "parties",
            principalColumn: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_parties_settlements_current_settlement_id",
            table: "parties",
            column: "current_settlement_id",
            principalTable: "settlements",
            principalColumn: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
