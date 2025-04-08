using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddClanActivityLogTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,server_joined,team_hit,user_created,user_deleted,user_renamed,user_rewarded")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,item_bought,item_broke,item_reforged,item_repaired,item_sold,item_upgraded,server_joined,team_hit,user_created,user_deleted,user_renamed,user_rewarded");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,item_bought,item_broke,item_reforged,item_repaired,item_sold,item_upgraded,server_joined,team_hit,user_created,user_deleted,user_renamed,user_rewarded")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,server_joined,team_hit,user_created,user_deleted,user_renamed,user_rewarded");
    }
}
