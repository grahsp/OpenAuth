using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAuth.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAudienceBackingField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Audience",
                table: "Audience",
                newName: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Audience",
                newName: "Audience");
        }
    }
}
