using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace ShoppingListService.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchVectorWithComputedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "CatalogItems",
                type: "tsvector",
                nullable: true)
                .Annotation("Npgsql:TsVectorConfig", "english")
                .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "CategoryName" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_SearchVector",
                table: "CatalogItems",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CatalogItems_SearchVector",
                table: "CatalogItems");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "CatalogItems");
        }
    }
}
