using System;
using Crpg.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddMarketplace : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,marketplace_listing_accepted,marketplace_listing_cancelled,marketplace_listing_created,marketplace_listing_expired,marketplace_listing_invalidated,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
            .Annotation("Npgsql:Enum:marketplace_listing_asset_side", "offered,requested")
            .Annotation("Npgsql:Enum:notification_type", "battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked_to_ex_participant,character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,marketplace_listing_accepted_to_buyer,marketplace_listing_accepted_to_seller,marketplace_listing_expired,marketplace_listing_invalidated,user_rewarded_to_user")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
            .OldAnnotation("Npgsql:Enum:notification_type", "battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked_to_ex_participant,character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,user_rewarded_to_user");

        migrationBuilder.CreateTable(
            name: "marketplace_listings",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                seller_id = table.Column<int>(type: "integer", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                gold_fee = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_marketplace_listings", x => x.id);
                table.ForeignKey(
                    name: "fk_marketplace_listings_users_seller_id",
                    column: x => x.seller_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "marketplace_listing_assets",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                marketplace_listing_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<MarketplaceListingAssetSide>(type: "marketplace_listing_asset_side", nullable: false),
                gold = table.Column<int>(type: "integer", nullable: false),
                heirloom_points = table.Column<int>(type: "integer", nullable: false),
                user_item_id = table.Column<int>(type: "integer", nullable: true),
                item_id = table.Column<string>(type: "text", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_marketplace_listing_assets", x => x.id);
                table.ForeignKey(
                    name: "fk_marketplace_listing_assets_items_item_id",
                    column: x => x.item_id,
                    principalTable: "items",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_marketplace_listing_assets_marketplace_listings_marketplace",
                    column: x => x.marketplace_listing_id,
                    principalTable: "marketplace_listings",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_marketplace_listing_assets_user_items_user_item_id",
                    column: x => x.user_item_id,
                    principalTable: "user_items",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_marketplace_listing_assets_item_id",
            table: "marketplace_listing_assets",
            column: "item_id");

        migrationBuilder.CreateIndex(
            name: "ix_marketplace_listing_assets_marketplace_listing_id_side",
            table: "marketplace_listing_assets",
            columns: new[] { "marketplace_listing_id", "side" });

        migrationBuilder.CreateIndex(
            name: "ix_marketplace_listing_assets_user_item_id",
            table: "marketplace_listing_assets",
            column: "user_item_id");

        migrationBuilder.CreateIndex(
            name: "ix_marketplace_listings_created_at",
            table: "marketplace_listings",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_marketplace_listings_expires_at",
            table: "marketplace_listings",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "ix_marketplace_listings_seller_id_created_at",
            table: "marketplace_listings",
            columns: new[] { "seller_id", "created_at" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
