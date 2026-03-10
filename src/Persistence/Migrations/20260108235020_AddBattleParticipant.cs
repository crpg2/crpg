using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddBattleParticipant : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "battle_mercenaries");

        migrationBuilder.RenameColumn(
            name: "mercenary_slots",
            table: "battle_fighters",
            newName: "participant_slots");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:battle_participant_type", "clan_member,mercenary,party")
            .Annotation("Npgsql:Enum:terrain_type", "barrier,deep_water,plain,shallow_water,sparse_forest,thick_forest")
            .OldAnnotation("Npgsql:Enum:terrain_type", "barrier,deep_water,shallow_water,sparse_forest,thick_forest");

        migrationBuilder.CreateTable(
            name: "battle_participants",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
                type = table.Column<BattleParticipantType>(type: "battle_participant_type", nullable: false),
                character_id = table.Column<int>(type: "integer", nullable: false),
                captain_fighter_id = table.Column<int>(type: "integer", nullable: false),
                mercenary_application_id = table.Column<int>(type: "integer", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_participants", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_participants_battle_fighters_captain_fighter_id",
                    column: x => x.captain_fighter_id,
                    principalTable: "battle_fighters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_participants_battle_mercenary_applications_mercenary",
                    column: x => x.mercenary_application_id,
                    principalTable: "battle_mercenary_applications",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_battle_participants_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_participants_characters_character_id",
                    column: x => x.character_id,
                    principalTable: "characters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_side_briefings",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
                note = table.Column<string>(type: "text", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_side_briefings", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_side_briefings_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "battle_participant_statistic",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                participant_id = table.Column<int>(type: "integer", nullable: false),
                participated = table.Column<bool>(type: "boolean", nullable: false),
                kills = table.Column<int>(type: "integer", nullable: false),
                assists = table.Column<int>(type: "integer", nullable: false),
                deaths = table.Column<int>(type: "integer", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_participant_statistic", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_participant_statistic_battle_participants_participan",
                    column: x => x.participant_id,
                    principalTable: "battle_participants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_participant_statistic_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_battle_participant_statistic_battle_id",
            table: "battle_participant_statistic",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_participant_statistic_participant_id",
            table: "battle_participant_statistic",
            column: "participant_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_battle_participants_battle_id",
            table: "battle_participants",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_participants_captain_fighter_id",
            table: "battle_participants",
            column: "captain_fighter_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_participants_character_id",
            table: "battle_participants",
            column: "character_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_participants_mercenary_application_id",
            table: "battle_participants",
            column: "mercenary_application_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_side_briefings_battle_id_side",
            table: "battle_side_briefings",
            columns: new[] { "battle_id", "side" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "battle_participant_statistic");

        migrationBuilder.DropTable(
            name: "battle_side_briefings");

        migrationBuilder.DropTable(
            name: "battle_participants");

        migrationBuilder.RenameColumn(
            name: "participant_slots",
            table: "battle_fighters",
            newName: "mercenary_slots");

        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:terrain_type", "barrier,deep_water,shallow_water,sparse_forest,thick_forest")
            .OldAnnotation("Npgsql:Enum:terrain_type", "barrier,deep_water,plain,shallow_water,sparse_forest,thick_forest");

        migrationBuilder.CreateTable(
            name: "battle_mercenaries",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                application_id = table.Column<int>(type: "integer", nullable: false),
                battle_id = table.Column<int>(type: "integer", nullable: false),
                captain_fighter_id = table.Column<int>(type: "integer", nullable: false),
                character_id = table.Column<int>(type: "integer", nullable: false),
                side = table.Column<BattleSide>(type: "battle_side", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_battle_mercenaries", x => x.id);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_battle_fighters_captain_fighter_id",
                    column: x => x.captain_fighter_id,
                    principalTable: "battle_fighters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_battle_mercenary_applications_applicatio",
                    column: x => x.application_id,
                    principalTable: "battle_mercenary_applications",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_battles_battle_id",
                    column: x => x.battle_id,
                    principalTable: "battles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_battle_mercenaries_characters_character_id",
                    column: x => x.character_id,
                    principalTable: "characters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_application_id",
            table: "battle_mercenaries",
            column: "application_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_battle_id",
            table: "battle_mercenaries",
            column: "battle_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_captain_fighter_id",
            table: "battle_mercenaries",
            column: "captain_fighter_id");

        migrationBuilder.CreateIndex(
            name: "ix_battle_mercenaries_character_id",
            table: "battle_mercenaries",
            column: "character_id");
    }
}
