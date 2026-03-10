using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class CaptainGameMode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_conquest,crpgdtv,crpg_duel,crpg_siege,crpg_team_deathmatch,crpg_skirmish,crpg_unknown_game_mode,crpg_captain")
            .OldAnnotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_conquest,crpgdtv,crpg_duel,crpg_siege,crpg_team_deathmatch,crpg_skirmish,crpg_unknown_game_mode");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_conquest,crpgdtv,crpg_duel,crpg_siege,crpg_team_deathmatch,crpg_skirmish,crpg_unknown_game_mode")
            .OldAnnotation("Npgsql:Enum:game_mode", "crpg_battle,crpg_conquest,crpgdtv,crpg_duel,crpg_siege,crpg_team_deathmatch,crpg_skirmish,crpg_unknown_game_mode,crpg_captain");
    }
}
