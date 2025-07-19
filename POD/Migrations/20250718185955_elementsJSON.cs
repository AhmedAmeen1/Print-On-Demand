using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POD.Migrations
{
    /// <inheritdoc />
    public partial class elementsJSON : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Elements",
                table: "ProductTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Elements",
                table: "CustomProducts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Elements",
                table: "ProductTemplates");

            migrationBuilder.DropColumn(
                name: "Elements",
                table: "CustomProducts");
        }
    }
}
