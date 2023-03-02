using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComaxRpUI.Migrations
{
    /// <inheritdoc />
    public partial class activation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "RpEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Managed",
                table: "RpEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "RpEntries");

            migrationBuilder.DropColumn(
                name: "Managed",
                table: "RpEntries");
        }
    }
}
