using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Crpg.Persistence.Migrations;

/// <inheritdoc />
public partial class AddIdempotencyKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:user_update_status", "started,completed");

        migrationBuilder.CreateTable(
            name: "idempotency_keys",
            columns: table => new
            {
                key = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                status = table.Column<string>(type: "user_update_status", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_idempotency_keys", x => x.key);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "idempotency_keys");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:user_update_status", "started,completed");
    }
}
