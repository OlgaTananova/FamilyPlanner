using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingListService.Migrations
{
    /// <inheritdoc />
    public partial class AddedSKU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "CatalogItems",
                newName: "SKU");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ShoppingListItems",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategorySKU",
                table: "CatalogItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategorySKU",
                table: "CatalogItems");

            migrationBuilder.RenameColumn(
                name: "SKU",
                table: "CatalogItems",
                newName: "CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ShoppingListItems",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
