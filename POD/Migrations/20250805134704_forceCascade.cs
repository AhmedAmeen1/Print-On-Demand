using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POD.Migrations
{
    public partial class forceCascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // User → Orders
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // User → SellerProfiles
            migrationBuilder.DropForeignKey(
                name: "FK_SellerProfiles_AspNetUsers_UserId",
                table: "SellerProfiles");
            migrationBuilder.AddForeignKey(
                name: "FK_SellerProfiles_AspNetUsers_UserId",
                table: "SellerProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // User → CustomProducts
            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts");
            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // User → CartItems
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_AspNetUsers_UserId",
                table: "CartItems");
            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_AspNetUsers_UserId",
                table: "CartItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Orders → Payments
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            // Orders → OrderItems
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");
            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            // SellerProfiles → ProductTemplates
            migrationBuilder.DropForeignKey(
                name: "FK_ProductTemplates_SellerProfiles_SellerProfileId",
                table: "ProductTemplates");
            migrationBuilder.AddForeignKey(
                name: "FK_ProductTemplates_SellerProfiles_SellerProfileId",
                table: "ProductTemplates",
                column: "SellerProfileId",
                principalTable: "SellerProfiles",
                principalColumn: "SellerProfileId",
                onDelete: ReferentialAction.Cascade);

            // ProductTemplates → CustomProducts
            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts");
            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts",
                column: "ProductTemplateId",
                principalTable: "ProductTemplates",
                principalColumn: "ProductTemplateId",
                onDelete: ReferentialAction.Cascade);

            // CustomProducts → CartItems
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CustomProducts_CustomProductId",
                table: "CartItems");
            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_CustomProducts_CustomProductId",
                table: "CartItems",
                column: "CustomProductId",
                principalTable: "CustomProducts",
                principalColumn: "CustomProductId",
                onDelete: ReferentialAction.Cascade);

            // CustomProducts → OrderItems
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse cascade deletes by restoring restrict delete behavior

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders");
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_SellerProfiles_AspNetUsers_UserId",
                table: "SellerProfiles");
            migrationBuilder.AddForeignKey(
                name: "FK_SellerProfiles_AspNetUsers_UserId",
                table: "SellerProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts");
            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_AspNetUsers_UserId",
                table: "CustomProducts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_AspNetUsers_UserId",
                table: "CartItems");
            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_AspNetUsers_UserId",
                table: "CartItems",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");
            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTemplates_SellerProfiles_SellerProfileId",
                table: "ProductTemplates");
            migrationBuilder.AddForeignKey(
                name: "FK_ProductTemplates_SellerProfiles_SellerProfileId",
                table: "ProductTemplates",
                column: "SellerProfileId",
                principalTable: "SellerProfiles",
                principalColumn: "SellerProfileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts");
            migrationBuilder.AddForeignKey(
                name: "FK_CustomProducts_ProductTemplates_ProductTemplateId",
                table: "CustomProducts",
                column: "ProductTemplateId",
                principalTable: "ProductTemplates",
                principalColumn: "ProductTemplateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CustomProducts_CustomProductId",
                table: "CartItems");
            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_CustomProducts_CustomProductId",
                table: "CartItems",
                column: "CustomProductId",
                principalTable: "CustomProducts",
                principalColumn: "CustomProductId",
                onDelete: ReferentialAction.Restrict);

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
