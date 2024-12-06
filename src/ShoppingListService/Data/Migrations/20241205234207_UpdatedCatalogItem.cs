using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingListService.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCatalogItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "CatalogItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "CatalogItems");
        }
    }
}
