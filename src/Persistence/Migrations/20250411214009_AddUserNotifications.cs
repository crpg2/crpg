using System;
using Crpg.Domain.Entities.Notifications;
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
            .Annotation("Npgsql:Enum:notification_state", "read,unread")
            .Annotation("Npgsql:Enum:notification_type", "character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,user_rewarded_to_user");

        migrationBuilder.CreateTable(
            name: "user_notifications",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                type = table.Column<NotificationType>(type: "notification_type", nullable: false),
                state = table.Column<NotificationState>(type: "notification_state", nullable: false),
                user_id = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_notifications", x => x.id);
                table.ForeignKey(
                    name: "fk_user_notifications_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "user_notification_metadata",
            columns: table => new
            {
                user_notification_id = table.Column<int>(type: "integer", nullable: false),
                key = table.Column<string>(type: "text", nullable: false),
                value = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_notification_metadata", x => new { x.user_notification_id, x.key });
                table.ForeignKey(
                    name: "fk_user_notification_metadata_user_notifications_user_notifica",
                    column: x => x.user_notification_id,
                    principalTable: "user_notifications",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_user_notifications_user_id",
            table: "user_notifications",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_notification_metadata");

        migrationBuilder.DropTable(
            name: "user_notifications");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:notification_state", "read,unread")
            .OldAnnotation("Npgsql:Enum:notification_type", "character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,user_rewarded_to_user");
    }
}
