using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POD.Migrations
{
    /// <inheritdoc />
    public partial class cascade2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_CustomProducts_CustomProductId",
                table: "OrderItems");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_CustomProducts_CustomProductId",
                table: "OrderItems",
                column: "CustomProductId",
                principalTable: "CustomProducts",
                principalColumn: "CustomProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_CustomProducts_CustomProductId",
                table: "OrderItems");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_CustomProducts_CustomProductId",
                table: "OrderItems",
                column: "CustomProductId",
                principalTable: "CustomProducts",
                principalColumn: "CustomProductId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
