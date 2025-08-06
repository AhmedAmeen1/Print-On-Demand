using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POD.Migrations
{
    /// <inheritdoc />
    public partial class cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts",
                column: "ProductTemplateId",
                principalTable: "ProductTemplates",
                principalColumn: "ProductTemplateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts",
                column: "ProductTemplateId",
                principalTable: "ProductTemplates",
                principalColumn: "ProductTemplateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
