using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFood.Presentation.Migrations
{
    /// <inheritdoc />
    public partial class AdminUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Tokens",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Admins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AdminId",
                table: "Tokens",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Admins_AdminId",
                table: "Tokens",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Admins_AdminId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_AdminId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Admins_Email",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
