using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFood.Presentation.Migrations
{
    /// <inheritdoc />
    public partial class FixCartRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Product_ProductId1",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Carts_CartId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CartId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductId1",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "CartItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CartId",
                table: "Organizations",
                column: "CartId",
                unique: true,
                filter: "[CartId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId1",
                table: "CartItems",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Product_ProductId1",
                table: "CartItems",
                column: "ProductId1",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Carts_CartId",
                table: "Organizations",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id");
        }
    }
}
