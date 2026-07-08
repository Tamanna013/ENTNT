using System;
using System.Collections.Generic;
using FleetMind.Api.Models;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Data.Seed
{
    public static class SampleDataSeedData
    {
        public static readonly List<Fleet> SampleFleets = new List<Fleet>
        {
            new Fleet
            {
                Name = "Pacific Trade Fleet",
                Description = "Major trade routes across the Pacific Ocean.",
                Status = FleetStatus.Active
            },
            new Fleet
            {
                Name = "Atlantic Bulk Carriers",
                Description = "Specialized in bulk cargo across the Atlantic.",
                Status = FleetStatus.Active
            },
            new Fleet
            {
                Name = "Nordic Container Line",
                Description = "Connecting Northern Europe and global ports.",
                Status = FleetStatus.Active
            }
        };

        public static List<Ship> GetSampleShips(List<Guid> fleetIds)
        {
            return new List<Ship>
            {
                new Ship { FleetId = fleetIds[0], Name = "Pacific Voyager", IMO = "9123456", Type = ShipType.ContainerShip, Status = ShipStatus.Active, YearBuilt = 2015, GrossTonnage = 150000, Flag = "Panama" },
                new Ship { FleetId = fleetIds[0], Name = "Pacific Pioneer", IMO = "9234567", Type = ShipType.ContainerShip, Status = ShipStatus.Active, YearBuilt = 2018, GrossTonnage = 145000, Flag = "Liberia" },
                new Ship { FleetId = fleetIds[0], Name = "Pacific Horizon", IMO = "9345678", Type = ShipType.Tanker, Status = ShipStatus.InMaintenance, YearBuilt = 2012, GrossTonnage = 110000, Flag = "Bahamas" },
                new Ship { FleetId = fleetIds[1], Name = "Atlantic Titan", IMO = "9456789", Type = ShipType.BulkCarrier, Status = ShipStatus.Active, YearBuilt = 2019, GrossTonnage = 85000, Flag = "Malta" },
                new Ship { FleetId = fleetIds[1], Name = "Atlantic Pearl", IMO = "9567890", Type = ShipType.BulkCarrier, Status = ShipStatus.Active, YearBuilt = 2020, GrossTonnage = 88000, Flag = "Liberia" },
                new Ship { FleetId = fleetIds[1], Name = "Atlantic Dawn", IMO = "9678901", Type = ShipType.RoRo, Status = ShipStatus.Docked, YearBuilt = 2016, GrossTonnage = 45000, Flag = "Cyprus" },
                new Ship { FleetId = fleetIds[2], Name = "Nordic Star", IMO = "9789012", Type = ShipType.ContainerShip, Status = ShipStatus.Active, YearBuilt = 2021, GrossTonnage = 180000, Flag = "Denmark" },
                new Ship { FleetId = fleetIds[2], Name = "Nordic Explorer", IMO = "9890123", Type = ShipType.GeneralCargo, Status = ShipStatus.Active, YearBuilt = 2014, GrossTonnage = 60000, Flag = "Norway" }
            };
        }

        public static List<CrewMember> GetSampleCrewMembers(List<Guid> shipIds)
        {
            var rand = new Random();
            var crew = new List<CrewMember>();

            var names = new[] { 
                ("John", "Smith"), ("Sarah", "Connor"), ("Anders", "Nielsen"), ("Elena", "Rostova"), 
                ("Mohammed", "Al-Fayed"), ("Li", "Wei"), ("Carlos", "Santana"), ("Emma", "Watson"),
                ("Lars", "Olsen"), ("Maria", "Garcia"), ("James", "Holden"), ("Naomi", "Nagata"),
                ("Amos", "Burton"), ("Alex", "Kamal"), ("Chrisjen", "Avasarala"), ("Bobbie", "Draper"),
                ("Joe", "Miller"), ("Juliette", "Mao"), ("Fred", "Johnson"), ("Camina", "Drummer")
            };

            var ranks = new[] { 
                CrewRank.Captain, CrewRank.ChiefOfficer, CrewRank.ChiefEngineer, CrewRank.SecondOfficer, 
                CrewRank.Deckhand, CrewRank.Cook, CrewRank.Cadet 
            };

            var nationalities = new[] { "USA", "UK", "Denmark", "Russia", "Egypt", "China", "Mexico", "Norway", "Spain", "Brazil", "India", "Philippines" };

            for (int i = 0; i < 20; i++)
            {
                var isAssigned = i < 12; // 12 assigned, 8 unassigned
                var shipId = isAssigned ? shipIds[rand.Next(shipIds.Count)] : (Guid?)null;

                crew.Add(new CrewMember
                {
                    FirstName = names[i].Item1,
                    LastName = names[i].Item2,
                    Rank = ranks[rand.Next(ranks.Length)],
                    Status = isAssigned ? CrewStatus.Active : CrewStatus.Unassigned,
                    Nationality = nationalities[rand.Next(nationalities.Length)],
                    LicenseNumber = $"LIC-{10000 + i}",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20 - rand.Next(30))),
                    HireDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-rand.Next(10))),
                    ShipId = shipId,
                    ContactEmail = $"{names[i].Item1.ToLower()}.{names[i].Item2.ToLower()}@example.com",
                    ContactPhone = $"+1555100{i:D4}"
                });
            }

            return crew;
        }
    }
}
