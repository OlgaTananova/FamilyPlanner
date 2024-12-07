using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingListService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedShoppingListItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItems_CatalogItemId",
                table: "ShoppingListItems",
                column: "CatalogItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingListItems_CatalogItems_CatalogItemId",
                table: "ShoppingListItems",
                column: "CatalogItemId",
                principalTable: "CatalogItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingListItems_CatalogItems_CatalogItemId",
                table: "ShoppingListItems");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingListItems_CatalogItemId",
                table: "ShoppingListItems");
        }
    }
}
