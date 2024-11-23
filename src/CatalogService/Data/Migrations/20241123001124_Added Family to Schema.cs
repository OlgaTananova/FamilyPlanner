using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFamilytoSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Family",
                table: "Items",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Family",
                table: "Categories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Family",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Family",
                table: "Categories");
        }
    }
}
