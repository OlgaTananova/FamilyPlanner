using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShoppingListService.Data;

public partial class AddFullTextSearchToCatalogItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add the SearchVector column
        migrationBuilder.AddColumn<string>(
            name: "SearchVector",
            table: "CatalogItems",
            type: "tsvector",
            nullable: true);

        // Populate the SearchVector column
        migrationBuilder.Sql(
            @"UPDATE ""CatalogItems"" 
              SET ""SearchVector"" = 
              to_tsvector('english', ""Name"" || ' ' || ""CategoryName"")");

        // Create a GIN index on the SearchVector column
        migrationBuilder.Sql(
            @"CREATE INDEX search_vector_idx 
              ON ""CatalogItems"" USING GIN(""SearchVector"")");

        // Create a trigger to keep SearchVector updated
        migrationBuilder.Sql(
            @"CREATE TRIGGER update_search_vector 
              BEFORE INSERT OR UPDATE ON ""CatalogItems""
              FOR EACH ROW 
              EXECUTE FUNCTION 
              tsvector_update_trigger(""SearchVector"", 'pg_catalog.english', ""Name"", ""CategoryName"")");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove the trigger
        migrationBuilder.Sql(@"DROP TRIGGER update_search_vector ON ""CatalogItems""");

        // Remove the GIN index
        migrationBuilder.Sql(@"DROP INDEX search_vector_idx");

        // Drop the SearchVector column
        migrationBuilder.DropColumn(
            name: "SearchVector",
            table: "CatalogItems");
    }
}
