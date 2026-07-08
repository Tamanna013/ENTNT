using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetMind.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelEfficiencyStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
CREATE OR ALTER PROCEDURE [dbo].[sp_GetFuelEfficiencyReport]
    @TrailingDays INT = 90
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        s.Id AS ShipId,
        s.Name AS ShipName,
        COALESCE(SUM(f.QuantityLiters), 0) AS TotalQuantityLiters,
        COALESCE(SUM(f.QuantityLiters * f.CostPerLiter), 0) AS TotalCost,
        COALESCE(SUM(f.QuantityLiters * f.CostPerLiter) / NULLIF(SUM(f.QuantityLiters), 0), 0) AS AverageCostPerLiter,
        COUNT(f.Id) AS LogCount
    FROM 
        Ships s
    LEFT JOIN 
        FuelLogs f ON s.Id = f.ShipId 
        AND f.IsDeleted = 0 
        AND f.RecordedDate >= DATEADD(day, -@TrailingDays, GETUTCDATE())
    WHERE 
        s.IsDeleted = 0
    GROUP BY 
        s.Id, s.Name;
END
            ";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetFuelEfficiencyReport]");
        }
    }
}
