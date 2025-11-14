using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueKeyInInspection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inspections_RentalId_Type",
                table: "inspections");

            migrationBuilder.CreateIndex(
                name: "IX_inspections_RentalId",
                table: "inspections",
                column: "RentalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_inspections_RentalId",
                table: "inspections");

            migrationBuilder.CreateIndex(
                name: "IX_inspections_RentalId_Type",
                table: "inspections",
                columns: new[] { "RentalId", "Type" },
                unique: true);
        }
    }
}
