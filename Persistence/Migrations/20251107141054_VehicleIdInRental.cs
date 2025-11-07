using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VehicleIdInRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_rentals_vehicles_VehicleId1",
                table: "rentals");

            migrationBuilder.DropIndex(
                name: "IX_rentals_VehicleId1",
                table: "rentals");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "rentals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId1",
                table: "rentals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_rentals_VehicleId1",
                table: "rentals",
                column: "VehicleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_rentals_vehicles_VehicleId1",
                table: "rentals",
                column: "VehicleId1",
                principalTable: "vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
