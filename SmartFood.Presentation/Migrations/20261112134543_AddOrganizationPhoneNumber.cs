using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFood.Presentation.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationPhoneNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Organizations");
        }
    }
}
