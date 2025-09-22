using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAuth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSigningKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SigningKeys_Clients_ClientId",
                table: "SigningKeys");

            migrationBuilder.DropIndex(
                name: "IX_SigningKeys_ClientId",
                table: "SigningKeys");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "SigningKeys");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId1",
                table: "SigningKeys",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_ClientId1",
                table: "SigningKeys",
                column: "ClientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SigningKeys_Clients_ClientId1",
                table: "SigningKeys",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SigningKeys_Clients_ClientId1",
                table: "SigningKeys");

            migrationBuilder.DropIndex(
                name: "IX_SigningKeys_ClientId1",
                table: "SigningKeys");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "SigningKeys");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "SigningKeys",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_ClientId",
                table: "SigningKeys",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_SigningKeys_Clients_ClientId",
                table: "SigningKeys",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
