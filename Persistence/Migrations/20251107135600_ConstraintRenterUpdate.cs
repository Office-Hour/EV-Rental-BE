using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConstraintRenterUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rentals_Score_Range",
                table: "rentals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rentals_Score_Range",
                table: "rentals",
                sql: "[Score] BETWEEN 0 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Rentals_Score_Range",
                table: "rentals");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Rentals_Score_Range",
                table: "rentals",
                sql: "[Score] BETWEEN 1 AND 5");
        }
    }
}
