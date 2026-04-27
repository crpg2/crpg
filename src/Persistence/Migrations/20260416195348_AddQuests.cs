using System;
using Crpg.Domain.Entities.Quests;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddQuests : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,quest_rerolled,quest_reward_claimed,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
            .Annotation("Npgsql:Enum:quest_aggregation_type", "count,sum")
            .Annotation("Npgsql:Enum:quest_type", "daily,weekly")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded");

        migrationBuilder.CreateTable(
            name: "game_events",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<int>(type: "integer", nullable: true),
                type = table.Column<int>(type: "integer", nullable: false),
                event_data = table.Column<string>(type: "jsonb", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_game_events", x => x.id);
                table.ForeignKey(
                    name: "fk_game_events_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "quest_definitions",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<QuestType>(type: "quest_type", nullable: false),
                event_type = table.Column<int>(type: "integer", nullable: false),
                event_filters_json = table.Column<string>(type: "jsonb", nullable: true),
                aggregation_type = table.Column<QuestAggregationType>(type: "quest_aggregation_type", nullable: false),
                aggregation_field = table.Column<int>(type: "integer", nullable: true),
                required_value = table.Column<int>(type: "integer", nullable: false),
                reward_gold = table.Column<int>(type: "integer", nullable: false),
                reward_experience = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_quest_definitions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "user_quests",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<int>(type: "integer", nullable: false),
                quest_definition_id = table.Column<int>(type: "integer", nullable: false),
                is_reward_claimed = table.Column<bool>(type: "boolean", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_quests", x => x.id);
                table.ForeignKey(
                    name: "fk_user_quests_quest_definitions_quest_definition_id",
                    column: x => x.quest_definition_id,
                    principalTable: "quest_definitions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "fk_user_quests_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "weekly_quest_assignments",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                quest_definition_id = table.Column<int>(type: "integer", nullable: false),
                assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_weekly_quest_assignments", x => x.id);
                table.ForeignKey(
                    name: "fk_weekly_quest_assignments_quest_definitions_quest_definition",
                    column: x => x.quest_definition_id,
                    principalTable: "quest_definitions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_game_events_created_at",
            table: "game_events",
            column: "created_at");

        migrationBuilder.CreateIndex(
            name: "ix_game_events_user_id_type_created_at",
            table: "game_events",
            columns: new[] { "user_id", "type", "created_at" });

        migrationBuilder.CreateIndex(
            name: "ix_user_quests_quest_definition_id",
            table: "user_quests",
            column: "quest_definition_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_quests_user_id",
            table: "user_quests",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ix_weekly_quest_assignments_expires_at",
            table: "weekly_quest_assignments",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "ix_weekly_quest_assignments_quest_definition_id",
            table: "weekly_quest_assignments",
            column: "quest_definition_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // there is no retreat
    }
}
