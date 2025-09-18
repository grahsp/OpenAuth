using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAuth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grants",
                table: "Clients");

            migrationBuilder.CreateTable(
                name: "Audience",
                columns: table => new
                {
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Audience = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audience", x => new { x.ClientId, x.Id });
                    table.ForeignKey(
                        name: "FK_Audience_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Scope",
                columns: table => new
                {
                    AudienceClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AudienceId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scope", x => new { x.AudienceClientId, x.AudienceId, x.Id });
                    table.ForeignKey(
                        name: "FK_Scope_Audience_AudienceClientId_AudienceId",
                        columns: x => new { x.AudienceClientId, x.AudienceId },
                        principalTable: "Audience",
                        principalColumns: new[] { "ClientId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scope");

            migrationBuilder.DropTable(
                name: "Audience");

            migrationBuilder.AddColumn<string>(
                name: "Grants",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
