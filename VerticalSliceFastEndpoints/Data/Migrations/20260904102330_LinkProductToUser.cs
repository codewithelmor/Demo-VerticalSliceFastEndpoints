using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticalSliceFastEndpoints.Data.Migrations
{
    /// <inheritdoc />
    public partial class LinkProductToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedByUserId",
                table: "Products",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DeletedByUserId",
                table: "Products",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UpdatedByUserId",
                table: "Products",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_CreatedByUserId",
                table: "Products",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_DeletedByUserId",
                table: "Products",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_UpdatedByUserId",
                table: "Products",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_CreatedByUserId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_DeletedByUserId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_UpdatedByUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedByUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_DeletedByUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UpdatedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Products");
        }
    }
}
