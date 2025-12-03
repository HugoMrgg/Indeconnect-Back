using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndeConnect_Back.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductCrudChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductDetail_Key",
                table: "ProductDetails");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ProductDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "ProductDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetail_Key",
                table: "ProductDetails",
                column: "Key");
        }
    }
}
