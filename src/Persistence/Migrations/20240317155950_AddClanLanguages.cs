﻿using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddClanLanguages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
            .Annotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .Annotation("Npgsql:Enum:battle_side", "attacker,defender")
            .Annotation("Npgsql:Enum:character_class", "peasant,infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer")
            .Annotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .Annotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .Annotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .Annotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .Annotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3,weapon_extra")
            .Annotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .Annotation("Npgsql:Enum:languages", "en,zh,ru,de,fr,it,es,pl,uk,ro,nl,tr,el,hu,sv,cs,pt,sr,bg,hr,da,fi,no,be,lv")
            .Annotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .Annotation("Npgsql:Enum:platform", "steam,epic_games,microsoft")
            .Annotation("Npgsql:Enum:region", "eu,na,as,oc")
            .Annotation("Npgsql:Enum:restriction_type", "all,join,chat")
            .Annotation("Npgsql:Enum:role", "user,moderator,game_admin,admin")
            .Annotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .Annotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .Annotation("Npgsql:PostgresExtension:postgis", ",,")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
            .OldAnnotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .OldAnnotation("Npgsql:Enum:battle_side", "attacker,defender")
            .OldAnnotation("Npgsql:Enum:character_class", "peasant,infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer")
            .OldAnnotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .OldAnnotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .OldAnnotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .OldAnnotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .OldAnnotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3,weapon_extra")
            .OldAnnotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .OldAnnotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .OldAnnotation("Npgsql:Enum:platform", "steam,epic_games,microsoft")
            .OldAnnotation("Npgsql:Enum:region", "eu,na,as,oc")
            .OldAnnotation("Npgsql:Enum:restriction_type", "all,join,chat")
            .OldAnnotation("Npgsql:Enum:role", "user,moderator,game_admin,admin")
            .OldAnnotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .OldAnnotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

        migrationBuilder.AddColumn<Languages[]>(
            name: "languages",
            table: "clans",
            type: "languages[]",
            nullable: false,
            defaultValue: new Languages[0]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "languages",
            table: "clans");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
            .Annotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .Annotation("Npgsql:Enum:battle_side", "attacker,defender")
            .Annotation("Npgsql:Enum:character_class", "peasant,infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer")
            .Annotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .Annotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .Annotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .Annotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .Annotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .Annotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3,weapon_extra")
            .Annotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .Annotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .Annotation("Npgsql:Enum:platform", "steam,epic_games,microsoft")
            .Annotation("Npgsql:Enum:region", "eu,na,as,oc")
            .Annotation("Npgsql:Enum:restriction_type", "all,join,chat")
            .Annotation("Npgsql:Enum:role", "user,moderator,game_admin,admin")
            .Annotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .Annotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .Annotation("Npgsql:PostgresExtension:postgis", ",,")
            .OldAnnotation("Npgsql:Enum:activity_log_type", "user_created,user_deleted,user_renamed,user_rewarded,item_bought,item_sold,item_broke,item_reforged,item_repaired,item_upgraded,character_created,character_deleted,character_rating_reset,character_respecialized,character_retired,character_rewarded,character_earned,server_joined,chat_message_sent,team_hit,clan_armory_add_item,clan_armory_remove_item,clan_armory_return_item,clan_armory_borrow_item")
            .OldAnnotation("Npgsql:Enum:battle_fighter_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_mercenary_application_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:battle_phase", "preparation,hiring,scheduled,live,end")
            .OldAnnotation("Npgsql:Enum:battle_side", "attacker,defender")
            .OldAnnotation("Npgsql:Enum:character_class", "peasant,infantry,shock_infantry,skirmisher,crossbowman,archer,cavalry,mounted_archer")
            .OldAnnotation("Npgsql:Enum:clan_invitation_status", "pending,declined,accepted")
            .OldAnnotation("Npgsql:Enum:clan_invitation_type", "request,offer")
            .OldAnnotation("Npgsql:Enum:clan_member_role", "member,officer,leader")
            .OldAnnotation("Npgsql:Enum:culture", "neutral,aserai,battania,empire,khuzait,looters,sturgia,vlandia")
            .OldAnnotation("Npgsql:Enum:damage_type", "undefined,cut,pierce,blunt")
            .OldAnnotation("Npgsql:Enum:item_slot", "head,shoulder,body,hand,leg,mount_harness,mount,weapon0,weapon1,weapon2,weapon3,weapon_extra")
            .OldAnnotation("Npgsql:Enum:item_type", "undefined,head_armor,shoulder_armor,body_armor,hand_armor,leg_armor,mount_harness,mount,shield,bow,crossbow,one_handed_weapon,two_handed_weapon,polearm,thrown,arrows,bolts,pistol,musket,bullets,banner")
            .OldAnnotation("Npgsql:Enum:languages", "en,zh,ru,de,fr,it,es,pl,uk,ro,nl,tr,el,hu,sv,cs,pt,sr,bg,hr,da,fi,no,be,lv")
            .OldAnnotation("Npgsql:Enum:party_status", "idle,idle_in_settlement,recruiting_in_settlement,moving_to_point,following_party,moving_to_settlement,moving_to_attack_party,moving_to_attack_settlement,in_battle")
            .OldAnnotation("Npgsql:Enum:platform", "steam,epic_games,microsoft")
            .OldAnnotation("Npgsql:Enum:region", "eu,na,as,oc")
            .OldAnnotation("Npgsql:Enum:restriction_type", "all,join,chat")
            .OldAnnotation("Npgsql:Enum:role", "user,moderator,game_admin,admin")
            .OldAnnotation("Npgsql:Enum:settlement_type", "village,castle,town")
            .OldAnnotation("Npgsql:Enum:weapon_class", "undefined,dagger,one_handed_sword,two_handed_sword,one_handed_axe,two_handed_axe,mace,pick,two_handed_mace,one_handed_polearm,two_handed_polearm,low_grip_polearm,arrow,bolt,cartridge,bow,crossbow,stone,boulder,throwing_axe,throwing_knife,javelin,pistol,musket,small_shield,large_shield,banner")
            .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
    }
}
