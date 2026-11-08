using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFood.Presentation.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenRelationWithRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Restaurants_RestaurantId",
                table: "Tokens");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Restaurants_RestaurantId",
                table: "Tokens",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Restaurants_RestaurantId",
                table: "Tokens");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Restaurants_RestaurantId",
                table: "Tokens",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");
        }
    }
}
