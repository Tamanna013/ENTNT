using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetMind.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelLogEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VoyageId",
                table: "Containers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FuelLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoyageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FuelType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    QuantityLiters = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    CostPerLiter = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    RecordedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelLogs_Ships_ShipId",
                        column: x => x.ShipId,
                        principalTable: "Ships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FuelLogs_Voyages_VoyageId",
                        column: x => x.VoyageId,
                        principalTable: "Voyages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Containers_VoyageId",
                table: "Containers",
                column: "VoyageId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelLogs_ShipId",
                table: "FuelLogs",
                column: "ShipId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelLogs_ShipId_RecordedDate",
                table: "FuelLogs",
                columns: new[] { "ShipId", "RecordedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FuelLogs_VoyageId",
                table: "FuelLogs",
                column: "VoyageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Containers_Voyages_VoyageId",
                table: "Containers",
                column: "VoyageId",
                principalTable: "Voyages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Containers_Voyages_VoyageId",
                table: "Containers");

            migrationBuilder.DropTable(
                name: "FuelLogs");

            migrationBuilder.DropIndex(
                name: "IX_Containers_VoyageId",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "VoyageId",
                table: "Containers");
        }
    }
}
