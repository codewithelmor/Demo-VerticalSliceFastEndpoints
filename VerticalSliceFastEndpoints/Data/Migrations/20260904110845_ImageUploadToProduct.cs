using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticalSliceFastEndpoints.Data.Migrations
{
    /// <inheritdoc />
    public partial class ImageUploadToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageContent",
                table: "Products",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUploadedByUserId",
                table: "Products",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImageUploadedDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ImageUploadedByUserId",
                table: "Products",
                column: "ImageUploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_ImageUploadedByUserId",
                table: "Products",
                column: "ImageUploadedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_ImageUploadedByUserId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ImageUploadedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageContent",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUploadedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUploadedDate",
                table: "Products");
        }
    }
}
