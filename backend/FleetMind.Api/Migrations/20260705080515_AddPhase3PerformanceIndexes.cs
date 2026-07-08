using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetMind.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase3PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Voyages_Status",
                table: "Voyages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Containers_Status",
                table: "Containers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cargo_Status",
                table: "Cargo",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Cargo_Type",
                table: "Cargo",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Voyages_Status",
                table: "Voyages");

            migrationBuilder.DropIndex(
                name: "IX_Containers_Status",
                table: "Containers");

            migrationBuilder.DropIndex(
                name: "IX_Cargo_Status",
                table: "Cargo");

            migrationBuilder.DropIndex(
                name: "IX_Cargo_Type",
                table: "Cargo");
        }
    }
}
