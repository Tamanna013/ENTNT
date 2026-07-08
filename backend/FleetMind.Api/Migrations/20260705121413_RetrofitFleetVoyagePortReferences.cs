using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetMind.Api.Data.Migrations
{
    public partial class RetrofitFleetVoyagePortReferences : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add Nullable columns
            migrationBuilder.AddColumn<Guid>(
                name: "HomePortId",
                table: "Fleets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OriginPortId",
                table: "Voyages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationPortId",
                table: "Voyages",
                type: "uniqueidentifier",
                nullable: true);

            // 2. Data backfill
            var backfillSql = @"
                -- Create missing ports for Fleets.HomePort
                INSERT INTO Ports (Id, Name, UnLocode, CreatedAt, UpdatedAt, IsDeleted)
                SELECT NEWID(), sub.HomePort, 'T' + RIGHT('000' + CAST(ROW_NUMBER() OVER (ORDER BY sub.HomePort) AS VARCHAR), 4), GETUTCDATE(), GETUTCDATE(), 0
                FROM (SELECT DISTINCT HomePort FROM Fleets WHERE HomePort IS NOT NULL AND HomePort <> '') sub
                WHERE NOT EXISTS (SELECT 1 FROM Ports WHERE Name = sub.HomePort);

                -- Create missing ports for Voyages.OriginPort
                INSERT INTO Ports (Id, Name, UnLocode, CreatedAt, UpdatedAt, IsDeleted)
                SELECT NEWID(), sub.OriginPort, 'O' + RIGHT('000' + CAST(ROW_NUMBER() OVER (ORDER BY sub.OriginPort) AS VARCHAR), 4), GETUTCDATE(), GETUTCDATE(), 0
                FROM (SELECT DISTINCT OriginPort FROM Voyages WHERE OriginPort IS NOT NULL AND OriginPort <> '') sub
                WHERE NOT EXISTS (SELECT 1 FROM Ports WHERE Name = sub.OriginPort);

                -- Create missing ports for Voyages.DestinationPort
                INSERT INTO Ports (Id, Name, UnLocode, CreatedAt, UpdatedAt, IsDeleted)
                SELECT NEWID(), sub.DestinationPort, 'D' + RIGHT('000' + CAST(ROW_NUMBER() OVER (ORDER BY sub.DestinationPort) AS VARCHAR), 4), GETUTCDATE(), GETUTCDATE(), 0
                FROM (SELECT DISTINCT DestinationPort FROM Voyages WHERE DestinationPort IS NOT NULL AND DestinationPort <> '') sub
                WHERE NOT EXISTS (SELECT 1 FROM Ports WHERE Name = sub.DestinationPort);

                -- Update Fleets
                UPDATE f
                SET f.HomePortId = p.Id
                FROM Fleets f
                JOIN Ports p ON f.HomePort = p.Name;

                -- Update Voyages (Origin)
                UPDATE v
                SET v.OriginPortId = p.Id
                FROM Voyages v
                JOIN Ports p ON v.OriginPort = p.Name;

                -- Update Voyages (Destination)
                UPDATE v
                SET v.DestinationPortId = p.Id
                FROM Voyages v
                JOIN Ports p ON v.DestinationPort = p.Name;
            ";
            migrationBuilder.Sql(backfillSql);

            // Give dummy ids to rows where ports were not found (if any were null/empty) to satisfy NOT NULL constraint
            var fixNullsSql = @"
                DECLARE @DummyPortId UNIQUEIDENTIFIER = NEWID();
                IF EXISTS (SELECT 1 FROM Fleets WHERE HomePortId IS NULL) OR EXISTS (SELECT 1 FROM Voyages WHERE OriginPortId IS NULL OR DestinationPortId IS NULL)
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM Ports WHERE Name = 'Unknown Port')
                    BEGIN
                        INSERT INTO Ports (Id, Name, UnLocode, CreatedAt, UpdatedAt, IsDeleted) VALUES (@DummyPortId, 'Unknown Port', 'T0000', GETUTCDATE(), GETUTCDATE(), 0);
                    END
                    ELSE
                    BEGIN
                        SELECT @DummyPortId = Id FROM Ports WHERE Name = 'Unknown Port';
                    END
                    UPDATE Fleets SET HomePortId = @DummyPortId WHERE HomePortId IS NULL;
                    UPDATE Voyages SET OriginPortId = @DummyPortId WHERE OriginPortId IS NULL;
                    UPDATE Voyages SET DestinationPortId = @DummyPortId WHERE DestinationPortId IS NULL;
                END
            ";
            migrationBuilder.Sql(fixNullsSql);

            // 3. Alter columns to NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "HomePortId",
                table: "Fleets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "OriginPortId",
                table: "Voyages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "DestinationPortId",
                table: "Voyages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // 4. Create Indexes and Foreign Keys
            migrationBuilder.CreateIndex(
                name: "IX_Fleets_HomePortId",
                table: "Fleets",
                column: "HomePortId");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_DestinationPortId",
                table: "Voyages",
                column: "DestinationPortId");

            migrationBuilder.CreateIndex(
                name: "IX_Voyages_OriginPortId",
                table: "Voyages",
                column: "OriginPortId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fleets_Ports_HomePortId",
                table: "Fleets",
                column: "HomePortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voyages_Ports_DestinationPortId",
                table: "Voyages",
                column: "DestinationPortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voyages_Ports_OriginPortId",
                table: "Voyages",
                column: "OriginPortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // 5. Drop old string columns
            migrationBuilder.DropColumn(
                name: "HomePort",
                table: "Fleets");

            migrationBuilder.DropColumn(
                name: "DestinationPort",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "OriginPort",
                table: "Voyages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: Data loss will occur if rolling back since string columns are recreated empty
            migrationBuilder.AddColumn<string>(
                name: "HomePort",
                table: "Fleets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DestinationPort",
                table: "Voyages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginPort",
                table: "Voyages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Reverse backfill
            var reverseSql = @"
                UPDATE f
                SET f.HomePort = p.Name
                FROM Fleets f
                JOIN Ports p ON f.HomePortId = p.Id;

                UPDATE v
                SET v.OriginPort = p.Name
                FROM Voyages v
                JOIN Ports p ON v.OriginPortId = p.Id;

                UPDATE v
                SET v.DestinationPort = p.Name
                FROM Voyages v
                JOIN Ports p ON v.DestinationPortId = p.Id;
            ";
            migrationBuilder.Sql(reverseSql);

            migrationBuilder.DropForeignKey(
                name: "FK_Fleets_Ports_HomePortId",
                table: "Fleets");

            migrationBuilder.DropForeignKey(
                name: "FK_Voyages_Ports_DestinationPortId",
                table: "Voyages");

            migrationBuilder.DropForeignKey(
                name: "FK_Voyages_Ports_OriginPortId",
                table: "Voyages");

            migrationBuilder.DropIndex(
                name: "IX_Fleets_HomePortId",
                table: "Fleets");

            migrationBuilder.DropIndex(
                name: "IX_Voyages_DestinationPortId",
                table: "Voyages");

            migrationBuilder.DropIndex(
                name: "IX_Voyages_OriginPortId",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "HomePortId",
                table: "Fleets");

            migrationBuilder.DropColumn(
                name: "DestinationPortId",
                table: "Voyages");

            migrationBuilder.DropColumn(
                name: "OriginPortId",
                table: "Voyages");
        }
    }
}
