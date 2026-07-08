-- Data backfill logic for RetrofitFleetVoyagePortReferences migration
-- This script creates missing Port rows from the existing string data in Fleets and Voyages,
-- then updates the new foreign key columns to point to those new Port rows.
-- NOTE: The UN/LOCODE values generated here ('T0001', 'O0001', 'D0001') are synthesized placeholders.
-- They require manual data cleanup for any real deployment.

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
