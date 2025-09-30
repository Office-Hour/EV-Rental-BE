using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackUri",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "FrontUri",
                table: "Document");

            migrationBuilder.AddColumn<Guid>(
                name: "BackImageId",
                table: "Document",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FrontImageId",
                table: "Document",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DomainBlob",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContainerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainBlob", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Document_BackImageId",
                table: "Document",
                column: "BackImageId",
                unique: true,
                filter: "[BackImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Document_FrontImageId",
                table: "Document",
                column: "FrontImageId",
                unique: true,
                filter: "[FrontImageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DomainBlob_BackImageId",
                table: "Document",
                column: "BackImageId",
                principalTable: "DomainBlob",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_DomainBlob_FrontImageId",
                table: "Document",
                column: "FrontImageId",
                principalTable: "DomainBlob",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_DomainBlob_BackImageId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_DomainBlob_FrontImageId",
                table: "Document");

            migrationBuilder.DropTable(
                name: "DomainBlob");

            migrationBuilder.DropIndex(
                name: "IX_Document_BackImageId",
                table: "Document");

            migrationBuilder.DropIndex(
                name: "IX_Document_FrontImageId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "BackImageId",
                table: "Document");

            migrationBuilder.DropColumn(
                name: "FrontImageId",
                table: "Document");

            migrationBuilder.AddColumn<string>(
                name: "BackUri",
                table: "Document",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FrontUri",
                table: "Document",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
