using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFood.Presentation.Migrations
{
    /// <inheritdoc />
    public partial class FixCodeOTPForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_VerificationDocument_Owner",
                table: "VerificationDocuments",
                sql: "(\r\n    OrgnizationId IS NOT NULL\r\n    AND RestaurantId IS NULL\r\n)\r\nOR\r\n(\r\n    OrgnizationId IS NULL\r\n    AND RestaurantId IS NOT NULL\r\n)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CodeOTP_Owner",
                table: "CodeOTPs",
                sql: "(\r\n    RestaurantId IS NOT NULL\r\n    AND OrganizationId IS NULL\r\n)\r\nOR\r\n(\r\n    RestaurantId IS NULL\r\n    AND OrganizationId IS NOT NULL\r\n)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_VerificationDocument_Owner",
                table: "VerificationDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CodeOTP_Owner",
                table: "CodeOTPs");
        }
    }
}
