using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crpg.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBattleGameServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "game_server",
                table: "battles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "game_server",
                table: "battles");
        }
    }
}
