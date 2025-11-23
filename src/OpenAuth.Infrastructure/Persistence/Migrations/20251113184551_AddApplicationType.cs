using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAuth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationType",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationType",
                table: "Clients");
        }
    }
}
