using System;
using Crpg.Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddUserNotifications : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,item_returned,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_created,clan_deleted,clan_application_created,clan_application_declined,clan_application_accepted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
            .Annotation("Npgsql:Enum:notification_state", "unread,read")
            .Annotation("Npgsql:Enum:notification_type", "user_rewarded_to_user,character_rewarded_to_user,item_returned,clan_application_created_to_user,clan_application_created_to_officers,clan_application_accepted_to_user,clan_application_declined_to_user,clan_member_role_changed_to_user,clan_member_leaved_to_leader,clan_member_kicked_to_ex_member,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
            ;

        migrationBuilder.CreateTable(
            name: "user_notifications",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<NotificationType>(type: "notification_type", nullable: false),
                state = table.Column<NotificationState>(type: "notification_state", nullable: false),
                user_id = table.Column<int>(type: "integer", nullable: false),
                activity_log_id = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_notifications", x => x.id);
                table.ForeignKey(
                    name: "fk_user_notifications_activity_logs_activity_log_id",
                    column: x => x.activity_log_id,
                    principalTable: "activity_logs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_user_notifications_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_notifications_activity_log_id",
            table: "user_notifications",
            column: "activity_log_id");

        migrationBuilder.CreateIndex(
            name: "ix_user_notifications_user_id",
            table: "user_notifications",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_notifications");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,item_returned,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_created,clan_deleted,clan_application_created,clan_application_declined,clan_application_accepted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
           ;
    }
}
