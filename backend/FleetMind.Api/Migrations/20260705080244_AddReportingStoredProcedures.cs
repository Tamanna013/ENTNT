using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetMind.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_GetFleetUtilizationReport
AS
BEGIN
    SET NOCOUNT ON;

    WITH ShipAgg AS (
        SELECT 
            FleetId,
            COUNT(Id) AS TotalShips,
            COUNT(CASE WHEN Status = 'Active' THEN Id END) AS ActiveShips,
            COUNT(CASE WHEN Status = 'InMaintenance' THEN Id END) AS ShipsInMaintenance
        FROM Ships
        WHERE IsDeleted = 0
        GROUP BY FleetId
    ),
    CrewAgg AS (
        SELECT 
            s.FleetId,
            COUNT(c.Id) AS TotalCrewAssigned
        FROM CrewMembers c
        INNER JOIN Ships s ON c.ShipId = s.Id AND s.IsDeleted = 0
        WHERE c.IsDeleted = 0
        GROUP BY s.FleetId
    ),
    VoyageAgg AS (
        SELECT 
            s.FleetId,
            COUNT(DISTINCT v.Id) AS TotalVoyagesLast90Days,
            COALESCE(SUM(ca.WeightKg), 0) AS TotalCargoWeightLast90Days
        FROM Voyages v
        INNER JOIN Ships s ON v.ShipId = s.Id AND s.IsDeleted = 0
        LEFT JOIN Cargo ca ON ca.VoyageId = v.Id AND ca.IsDeleted = 0
        WHERE v.IsDeleted = 0 AND v.DepartureDate >= DATEADD(day, -90, GETUTCDATE())
        GROUP BY s.FleetId
    )
    SELECT 
        f.Id AS FleetId,
        f.Name AS FleetName,
        COALESCE(s.TotalShips, 0) AS TotalShips,
        COALESCE(s.ActiveShips, 0) AS ActiveShips,
        COALESCE(s.ShipsInMaintenance, 0) AS ShipsInMaintenance,
        COALESCE(c.TotalCrewAssigned, 0) AS TotalCrewAssigned,
        COALESCE(v.TotalVoyagesLast90Days, 0) AS TotalVoyagesLast90Days,
        COALESCE(v.TotalCargoWeightLast90Days, 0) AS TotalCargoWeightLast90Days
    FROM 
        Fleets f
    LEFT JOIN ShipAgg s ON s.FleetId = f.Id
    LEFT JOIN CrewAgg c ON c.FleetId = f.Id
    LEFT JOIN VoyageAgg v ON v.FleetId = f.Id
    WHERE 
        f.IsDeleted = 0;
END;
");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_GetVoyageManifestReport
    @VoyageId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Id AS CargoId,
        c.Description,
        c.Type,
        c.Status,
        c.WeightKg,
        c.DeclaredValue,
        c.ConsigneeName,
        cont.ContainerNumber
    FROM 
        Cargo c
    LEFT JOIN 
        ContainerCargoItems cci ON cci.CargoId = c.Id
    LEFT JOIN 
        Containers cont ON cont.Id = cci.ContainerId AND cont.IsDeleted = 0
    WHERE 
        c.VoyageId = @VoyageId AND c.IsDeleted = 0;
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetFleetUtilizationReport;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetVoyageManifestReport;");
        }
    }
}
