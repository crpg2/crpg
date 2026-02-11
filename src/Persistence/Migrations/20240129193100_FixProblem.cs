using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class FixProblem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_borrowed_items_clan_members_borrower_temp_id",
            table: "clan_armory_borrowed_items");

        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_borrowed_items_clans_clan_id",
            table: "clan_armory_borrowed_items");

        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_items_clan_members_lender_temp_id1",
            table: "clan_armory_items");

        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_items_clans_clan_id",
            table: "clan_armory_items");

        migrationBuilder.DropForeignKey(
            name: "fk_parties_users_user_id",
            table: "parties");

        migrationBuilder.DropForeignKey(
            name: "fk_users_characters_active_character_id1",
            table: "users");

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_borrowed_items_clan_members_borrower_user_id",
            table: "clan_armory_borrowed_items",
            column: "borrower_user_id",
            principalTable: "clan_members",
            principalColumn: "user_id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_borrowed_items_clans_borrower_clan_id",
            table: "clan_armory_borrowed_items",
            column: "borrower_clan_id",
            principalTable: "clans",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_items_clan_members_lender_user_id",
            table: "clan_armory_items",
            column: "lender_user_id",
            principalTable: "clan_members",
            principalColumn: "user_id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_items_clans_lender_clan_id",
            table: "clan_armory_items",
            column: "lender_clan_id",
            principalTable: "clans",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_parties_users_id",
            table: "parties",
            column: "id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_users_characters_active_character_id",
            table: "users",
            column: "active_character_id",
            principalTable: "characters",
            principalColumn: "id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_borrowed_items_clan_members_borrower_user_id",
            table: "clan_armory_borrowed_items");

        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_borrowed_items_clans_borrower_clan_id",
            table: "clan_armory_borrowed_items");

        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_items_clan_members_lender_user_id",
            table: "clan_armory_items");

        migrationBuilder.DropForeignKey(
            name: "fk_clan_armory_items_clans_lender_clan_id",
            table: "clan_armory_items");

        migrationBuilder.DropForeignKey(
            name: "fk_parties_users_id",
            table: "parties");

        migrationBuilder.DropForeignKey(
            name: "fk_users_characters_active_character_id",
            table: "users");

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_borrowed_items_clan_members_borrower_temp_id",
            table: "clan_armory_borrowed_items",
            column: "borrower_user_id",
            principalTable: "clan_members",
            principalColumn: "user_id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_borrowed_items_clans_clan_id",
            table: "clan_armory_borrowed_items",
            column: "borrower_clan_id",
            principalTable: "clans",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_items_clan_members_lender_temp_id1",
            table: "clan_armory_items",
            column: "lender_user_id",
            principalTable: "clan_members",
            principalColumn: "user_id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_clan_armory_items_clans_clan_id",
            table: "clan_armory_items",
            column: "lender_clan_id",
            principalTable: "clans",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_parties_users_user_id",
            table: "parties",
            column: "id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_users_characters_active_character_id1",
            table: "users",
            column: "active_character_id",
            principalTable: "characters",
            principalColumn: "id");
    }
}
