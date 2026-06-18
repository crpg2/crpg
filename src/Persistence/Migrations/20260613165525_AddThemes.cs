using System;
using System.Collections.Generic;
using Crpg.Domain.Entities.Themes;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddThemes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,marketplace_listing_accepted,marketplace_listing_cancelled,marketplace_listing_created,marketplace_listing_expired,marketplace_listing_invalidated,quest_rerolled,quest_reward_claimed,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
                .Annotation("Npgsql:Enum:battle_fighter_application_status", "accepted,declined,intent,pending")
                .Annotation("Npgsql:Enum:battle_mercenary_application_status", "accepted,declined,pending")
                .Annotation("Npgsql:Enum:battle_participant_type", "clan_member,mercenary,party")
                .Annotation("Npgsql:Enum:battle_phase", "end,hiring,live,preparation,scheduled")
                .Annotation("Npgsql:Enum:battle_side", "attacker,defender")
                .Annotation("Npgsql:Enum:character_class", "archer,cavalry,crossbowman,infantry,mounted_archer,peasant,shock_infantry,skirmisher")
                .Annotation("Npgsql:Enum:clan_invitation_status", "accepted,declined,pending")
                .Annotation("Npgsql:Enum:clan_invitation_type", "offer,request")
                .Annotation("Npgsql:Enum:clan_member_role", "leader,member,officer")
                .Annotation("Npgsql:Enum:culture", "aserai,battania,empire,khuzait,looters,neutral,sturgia,vlandia")
                .Annotation("Npgsql:Enum:damage_type", "blunt,cut,pierce,undefined")
                .Annotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_captain,crpg_conquest,crpg_duel,crpg_siege,crpg_skirmish,crpg_team_deathmatch,crpg_unknown_game_mode,crpgdtv")
                .Annotation("Npgsql:Enum:item_slot", "body,hand,head,leg,mount,mount_harness,shoulder,weapon0,weapon1,weapon2,weapon3,weapon_extra")
                .Annotation("Npgsql:Enum:item_type", "arrows,banner,body_armor,bolts,bow,bullets,crossbow,hand_armor,head_armor,leg_armor,mount,mount_harness,musket,one_handed_weapon,pistol,polearm,shield,shoulder_armor,thrown,two_handed_weapon,undefined")
                .Annotation("Npgsql:Enum:languages", "be,bg,cs,da,de,el,en,es,fi,fr,hr,hu,it,lv,nl,no,pl,pt,ro,ru,sr,sv,tr,uk,zh")
                .Annotation("Npgsql:Enum:marketplace_listing_asset_side", "offered,requested")
                .Annotation("Npgsql:Enum:notification_state", "read,unread")
                .Annotation("Npgsql:Enum:notification_type", "battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked_to_ex_participant,character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,marketplace_listing_accepted_to_buyer,marketplace_listing_accepted_to_seller,marketplace_listing_expired,marketplace_listing_invalidated,user_rewarded_to_user")
                .Annotation("Npgsql:Enum:party_order_type", "attack_party,attack_settlement,follow_party,join_battle,move_to_point,move_to_settlement,transfer_offer_party")
                .Annotation("Npgsql:Enum:party_status", "awaiting_battle_join_decision,awaiting_party_offer_decision,idle,idle_in_settlement,in_battle,recruiting_in_settlement")
                .Annotation("Npgsql:Enum:party_transfer_offer_status", "intent,pending")
                .Annotation("Npgsql:Enum:platform", "epic_games,microsoft,steam")
                .Annotation("Npgsql:Enum:quest_aggregation_type", "count,sum")
                .Annotation("Npgsql:Enum:quest_type", "daily,weekly")
                .Annotation("Npgsql:Enum:region", "as,eu,na,oc")
                .Annotation("Npgsql:Enum:restriction_type", "all,chat,join")
                .Annotation("Npgsql:Enum:role", "admin,game_admin,moderator,user")
                .Annotation("Npgsql:Enum:settlement_type", "castle,town,village")
                .Annotation("Npgsql:Enum:terrain_type", "barrier,deep_water,plain,shallow_water,sparse_forest,thick_forest")
                .Annotation("Npgsql:Enum:theme_equipment_slot", "body,hand,head,leg,mount,mount_harness,shoulder,weapon0,weapon1,weapon2,weapon3,weapon_extra")
                .Annotation("Npgsql:Enum:weapon_class", "arrow,banner,bolt,boulder,bow,cartridge,crossbow,dagger,javelin,large_shield,low_grip_polearm,mace,musket,one_handed_axe,one_handed_polearm,one_handed_sword,pick,pistol,small_shield,stone,throwing_axe,throwing_knife,two_handed_axe,two_handed_mace,two_handed_polearm,two_handed_sword,undefined")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,marketplace_listing_accepted,marketplace_listing_cancelled,marketplace_listing_created,marketplace_listing_expired,marketplace_listing_invalidated,quest_rerolled,quest_reward_claimed,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
                .OldAnnotation("Npgsql:Enum:battle_fighter_application_status", "accepted,declined,intent,pending")
                .OldAnnotation("Npgsql:Enum:battle_mercenary_application_status", "accepted,declined,pending")
                .OldAnnotation("Npgsql:Enum:battle_participant_type", "clan_member,mercenary,party")
                .OldAnnotation("Npgsql:Enum:battle_phase", "end,hiring,live,preparation,scheduled")
                .OldAnnotation("Npgsql:Enum:battle_side", "attacker,defender")
                .OldAnnotation("Npgsql:Enum:character_class", "archer,cavalry,crossbowman,infantry,mounted_archer,peasant,shock_infantry,skirmisher")
                .OldAnnotation("Npgsql:Enum:clan_invitation_status", "accepted,declined,pending")
                .OldAnnotation("Npgsql:Enum:clan_invitation_type", "offer,request")
                .OldAnnotation("Npgsql:Enum:clan_member_role", "leader,member,officer")
                .OldAnnotation("Npgsql:Enum:culture", "aserai,battania,empire,khuzait,looters,neutral,sturgia,vlandia")
                .OldAnnotation("Npgsql:Enum:damage_type", "blunt,cut,pierce,undefined")
                .OldAnnotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_captain,crpg_conquest,crpg_duel,crpg_siege,crpg_skirmish,crpg_team_deathmatch,crpg_unknown_game_mode,crpgdtv")
                .OldAnnotation("Npgsql:Enum:item_slot", "body,hand,head,leg,mount,mount_harness,shoulder,weapon0,weapon1,weapon2,weapon3,weapon_extra")
                .OldAnnotation("Npgsql:Enum:item_type", "arrows,banner,body_armor,bolts,bow,bullets,crossbow,hand_armor,head_armor,leg_armor,mount,mount_harness,musket,one_handed_weapon,pistol,polearm,shield,shoulder_armor,thrown,two_handed_weapon,undefined")
                .OldAnnotation("Npgsql:Enum:languages", "be,bg,cs,da,de,el,en,es,fi,fr,hr,hu,it,lv,nl,no,pl,pt,ro,ru,sr,sv,tr,uk,zh")
                .OldAnnotation("Npgsql:Enum:marketplace_listing_asset_side", "offered,requested")
                .OldAnnotation("Npgsql:Enum:notification_state", "read,unread")
                .OldAnnotation("Npgsql:Enum:notification_type", "battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked_to_ex_participant,character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,marketplace_listing_accepted_to_buyer,marketplace_listing_accepted_to_seller,marketplace_listing_expired,marketplace_listing_invalidated,user_rewarded_to_user")
                .OldAnnotation("Npgsql:Enum:party_order_type", "attack_party,attack_settlement,follow_party,join_battle,move_to_point,move_to_settlement,transfer_offer_party")
                .OldAnnotation("Npgsql:Enum:party_status", "awaiting_battle_join_decision,awaiting_party_offer_decision,idle,idle_in_settlement,in_battle,recruiting_in_settlement")
                .OldAnnotation("Npgsql:Enum:party_transfer_offer_status", "intent,pending")
                .OldAnnotation("Npgsql:Enum:platform", "epic_games,microsoft,steam")
                .OldAnnotation("Npgsql:Enum:quest_aggregation_type", "count,sum")
                .OldAnnotation("Npgsql:Enum:quest_type", "daily,weekly")
                .OldAnnotation("Npgsql:Enum:region", "as,eu,na,oc")
                .OldAnnotation("Npgsql:Enum:restriction_type", "all,chat,join")
                .OldAnnotation("Npgsql:Enum:role", "admin,game_admin,moderator,user")
                .OldAnnotation("Npgsql:Enum:settlement_type", "castle,town,village")
                .OldAnnotation("Npgsql:Enum:terrain_type", "barrier,deep_water,plain,shallow_water,sparse_forest,thick_forest")
                .OldAnnotation("Npgsql:Enum:weapon_class", "arrow,banner,bolt,boulder,bow,cartridge,crossbow,dagger,javelin,large_shield,low_grip_polearm,mace,musket,one_handed_axe,one_handed_polearm,one_handed_sword,pick,pistol,small_shield,stone,throwing_axe,throwing_knife,two_handed_axe,two_handed_mace,two_handed_polearm,two_handed_sword,undefined")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "themes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_themes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "theme_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gold_multiplier = table.Column<float>(type: "real", nullable: false),
                    exp_multiplier = table.Column<float>(type: "real", nullable: false),
                    active_from_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    active_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    required_equipment_slots_matching_theme = table.Column<List<ThemeEquipmentSlot>>(type: "theme_equipment_slot[]", nullable: false),
                    minimum_themed_items_equipped = table.Column<int>(type: "integer", nullable: false),
                    event_theme_id = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_theme_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_theme_events_themes_event_theme_id",
                        column: x => x.event_theme_id,
                        principalTable: "themes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_theme_events_event_theme_id",
                table: "theme_events",
                column: "event_theme_id");

            migrationBuilder.CreateIndex(
                name: "ix_theme_events_name",
                table: "theme_events",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_themes_name",
                table: "themes",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "theme_events");

            migrationBuilder.DropTable(
                name: "themes");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,marketplace_listing_accepted,marketplace_listing_cancelled,marketplace_listing_created,marketplace_listing_expired,marketplace_listing_invalidated,quest_rerolled,quest_reward_claimed,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
                .Annotation("Npgsql:Enum:battle_fighter_application_status", "accepted,declined,intent,pending")
                .Annotation("Npgsql:Enum:battle_mercenary_application_status", "accepted,declined,pending")
                .Annotation("Npgsql:Enum:battle_participant_type", "clan_member,mercenary,party")
                .Annotation("Npgsql:Enum:battle_phase", "end,hiring,live,preparation,scheduled")
                .Annotation("Npgsql:Enum:battle_side", "attacker,defender")
                .Annotation("Npgsql:Enum:character_class", "archer,cavalry,crossbowman,infantry,mounted_archer,peasant,shock_infantry,skirmisher")
                .Annotation("Npgsql:Enum:clan_invitation_status", "accepted,declined,pending")
                .Annotation("Npgsql:Enum:clan_invitation_type", "offer,request")
                .Annotation("Npgsql:Enum:clan_member_role", "leader,member,officer")
                .Annotation("Npgsql:Enum:culture", "aserai,battania,empire,khuzait,looters,neutral,sturgia,vlandia")
                .Annotation("Npgsql:Enum:damage_type", "blunt,cut,pierce,undefined")
                .Annotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_captain,crpg_conquest,crpg_duel,crpg_siege,crpg_skirmish,crpg_team_deathmatch,crpg_unknown_game_mode,crpgdtv")
                .Annotation("Npgsql:Enum:item_slot", "body,hand,head,leg,mount,mount_harness,shoulder,weapon0,weapon1,weapon2,weapon3,weapon_extra")
                .Annotation("Npgsql:Enum:item_type", "arrows,banner,body_armor,bolts,bow,bullets,crossbow,hand_armor,head_armor,leg_armor,mount,mount_harness,musket,one_handed_weapon,pistol,polearm,shield,shoulder_armor,thrown,two_handed_weapon,undefined")
                .Annotation("Npgsql:Enum:languages", "be,bg,cs,da,de,el,en,es,fi,fr,hr,hu,it,lv,nl,no,pl,pt,ro,ru,sr,sv,tr,uk,zh")
                .Annotation("Npgsql:Enum:marketplace_listing_asset_side", "offered,requested")
                .Annotation("Npgsql:Enum:notification_state", "read,unread")
                .Annotation("Npgsql:Enum:notification_type", "battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked_to_ex_participant,character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,marketplace_listing_accepted_to_buyer,marketplace_listing_accepted_to_seller,marketplace_listing_expired,marketplace_listing_invalidated,user_rewarded_to_user")
                .Annotation("Npgsql:Enum:party_order_type", "attack_party,attack_settlement,follow_party,join_battle,move_to_point,move_to_settlement,transfer_offer_party")
                .Annotation("Npgsql:Enum:party_status", "awaiting_battle_join_decision,awaiting_party_offer_decision,idle,idle_in_settlement,in_battle,recruiting_in_settlement")
                .Annotation("Npgsql:Enum:party_transfer_offer_status", "intent,pending")
                .Annotation("Npgsql:Enum:platform", "epic_games,microsoft,steam")
                .Annotation("Npgsql:Enum:quest_aggregation_type", "count,sum")
                .Annotation("Npgsql:Enum:quest_type", "daily,weekly")
                .Annotation("Npgsql:Enum:region", "as,eu,na,oc")
                .Annotation("Npgsql:Enum:restriction_type", "all,chat,join")
                .Annotation("Npgsql:Enum:role", "admin,game_admin,moderator,user")
                .Annotation("Npgsql:Enum:settlement_type", "castle,town,village")
                .Annotation("Npgsql:Enum:terrain_type", "barrier,deep_water,plain,shallow_water,sparse_forest,thick_forest")
                .Annotation("Npgsql:Enum:weapon_class", "arrow,banner,bolt,boulder,bow,cartridge,crossbow,dagger,javelin,large_shield,low_grip_polearm,mace,musket,one_handed_axe,one_handed_polearm,one_handed_sword,pick,pistol,small_shield,stone,throwing_axe,throwing_knife,two_handed_axe,two_handed_mace,two_handed_polearm,two_handed_sword,undefined")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:activity_log_type", "battle_apply_as_mercenary,battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked,battle_participant_leaved,character_created,character_deleted,character_earned,character_rating_reset,character_respecialized,character_retired,character_rewarded,chat_message_sent,clan_application_accepted,clan_application_created,clan_application_declined,clan_armory_add_item,clan_armory_borrow_item,clan_armory_remove_item,clan_armory_return_item,clan_created,clan_deleted,clan_member_kicked,clan_member_leaved,clan_member_role_edited,item_bought,item_broke,item_reforged,item_repaired,item_returned,item_sold,item_upgraded,marketplace_listing_accepted,marketplace_listing_cancelled,marketplace_listing_created,marketplace_listing_expired,marketplace_listing_invalidated,quest_rerolled,quest_reward_claimed,server_joined,team_hit,team_hit_reported,team_hit_reported_user_kicked,user_created,user_deleted,user_renamed,user_rewarded")
                .OldAnnotation("Npgsql:Enum:battle_fighter_application_status", "accepted,declined,intent,pending")
                .OldAnnotation("Npgsql:Enum:battle_mercenary_application_status", "accepted,declined,pending")
                .OldAnnotation("Npgsql:Enum:battle_participant_type", "clan_member,mercenary,party")
                .OldAnnotation("Npgsql:Enum:battle_phase", "end,hiring,live,preparation,scheduled")
                .OldAnnotation("Npgsql:Enum:battle_side", "attacker,defender")
                .OldAnnotation("Npgsql:Enum:character_class", "archer,cavalry,crossbowman,infantry,mounted_archer,peasant,shock_infantry,skirmisher")
                .OldAnnotation("Npgsql:Enum:clan_invitation_status", "accepted,declined,pending")
                .OldAnnotation("Npgsql:Enum:clan_invitation_type", "offer,request")
                .OldAnnotation("Npgsql:Enum:clan_member_role", "leader,member,officer")
                .OldAnnotation("Npgsql:Enum:culture", "aserai,battania,empire,khuzait,looters,neutral,sturgia,vlandia")
                .OldAnnotation("Npgsql:Enum:damage_type", "blunt,cut,pierce,undefined")
                .OldAnnotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_captain,crpg_conquest,crpg_duel,crpg_siege,crpg_skirmish,crpg_team_deathmatch,crpg_unknown_game_mode,crpgdtv")
                .OldAnnotation("Npgsql:Enum:item_slot", "body,hand,head,leg,mount,mount_harness,shoulder,weapon0,weapon1,weapon2,weapon3,weapon_extra")
                .OldAnnotation("Npgsql:Enum:item_type", "arrows,banner,body_armor,bolts,bow,bullets,crossbow,hand_armor,head_armor,leg_armor,mount,mount_harness,musket,one_handed_weapon,pistol,polearm,shield,shoulder_armor,thrown,two_handed_weapon,undefined")
                .OldAnnotation("Npgsql:Enum:languages", "be,bg,cs,da,de,el,en,es,fi,fr,hr,hu,it,lv,nl,no,pl,pt,ro,ru,sr,sv,tr,uk,zh")
                .OldAnnotation("Npgsql:Enum:marketplace_listing_asset_side", "offered,requested")
                .OldAnnotation("Npgsql:Enum:notification_state", "read,unread")
                .OldAnnotation("Npgsql:Enum:notification_type", "battle_mercenary_application_accepted,battle_mercenary_application_declined,battle_participant_kicked_to_ex_participant,character_rewarded_to_user,clan_application_accepted_to_user,clan_application_created_to_officers,clan_application_created_to_user,clan_application_declined_to_user,clan_armory_borrow_item_to_lender,clan_armory_remove_item_to_borrower,clan_member_kicked_to_ex_member,clan_member_leaved_to_leader,clan_member_role_changed_to_user,item_returned,marketplace_listing_accepted_to_buyer,marketplace_listing_accepted_to_seller,marketplace_listing_expired,marketplace_listing_invalidated,user_rewarded_to_user")
                .OldAnnotation("Npgsql:Enum:party_order_type", "attack_party,attack_settlement,follow_party,join_battle,move_to_point,move_to_settlement,transfer_offer_party")
                .OldAnnotation("Npgsql:Enum:party_status", "awaiting_battle_join_decision,awaiting_party_offer_decision,idle,idle_in_settlement,in_battle,recruiting_in_settlement")
                .OldAnnotation("Npgsql:Enum:party_transfer_offer_status", "intent,pending")
                .OldAnnotation("Npgsql:Enum:platform", "epic_games,microsoft,steam")
                .OldAnnotation("Npgsql:Enum:quest_aggregation_type", "count,sum")
                .OldAnnotation("Npgsql:Enum:quest_type", "daily,weekly")
                .OldAnnotation("Npgsql:Enum:region", "as,eu,na,oc")
                .OldAnnotation("Npgsql:Enum:restriction_type", "all,chat,join")
                .OldAnnotation("Npgsql:Enum:role", "admin,game_admin,moderator,user")
                .OldAnnotation("Npgsql:Enum:settlement_type", "castle,town,village")
                .OldAnnotation("Npgsql:Enum:terrain_type", "barrier,deep_water,plain,shallow_water,sparse_forest,thick_forest")
                .OldAnnotation("Npgsql:Enum:theme_equipment_slot", "body,hand,head,leg,mount,mount_harness,shoulder,weapon0,weapon1,weapon2,weapon3,weapon_extra")
                .OldAnnotation("Npgsql:Enum:weapon_class", "arrow,banner,bolt,boulder,bow,cartridge,crossbow,dagger,javelin,large_shield,low_grip_polearm,mace,musket,one_handed_axe,one_handed_polearm,one_handed_sword,pick,pistol,small_shield,stone,throwing_axe,throwing_knife,two_handed_axe,two_handed_mace,two_handed_polearm,two_handed_sword,undefined")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
