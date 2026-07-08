using System;
using System.Collections.Generic;
using FleetMind.Api.Models;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Data.Seed
{
    public static class Phase4SampleDataSeedData
    {
        public static List<Port> GetSamplePorts()
        {
            return new List<Port>
            {
                new Port
                {
                    Name = "Port of Los Angeles",
                    UnLocode = "USLAX",
                    Country = "United States",
                    City = "Los Angeles",
                    Latitude = 33.7291m,
                    Longitude = -118.2620m
                },
                new Port
                {
                    Name = "Port of Rotterdam",
                    UnLocode = "NLRTM",
                    Country = "Netherlands",
                    City = "Rotterdam",
                    Latitude = 51.8850m,
                    Longitude = 4.2867m
                },
                new Port
                {
                    Name = "Port of Singapore",
                    UnLocode = "SGSIN",
                    Country = "Singapore",
                    City = "Singapore",
                    Latitude = 1.2640m,
                    Longitude = 103.8400m
                },
                new Port
                {
                    Name = "Port of Shanghai",
                    UnLocode = "CNSHA",
                    Country = "China",
                    City = "Shanghai",
                    Latitude = 31.2304m,
                    Longitude = 121.4737m
                },
                new Port
                {
                    Name = "Port of Hamburg",
                    UnLocode = "DEHAM",
                    Country = "Germany",
                    City = "Hamburg",
                    Latitude = 53.5488m,
                    Longitude = 9.9872m
                },
                new Port
                {
                    Name = "Port of Dubai (Jebel Ali)",
                    UnLocode = "AEJEA",
                    Country = "United Arab Emirates",
                    City = "Dubai",
                    Latitude = 24.9857m,
                    Longitude = 55.0273m
                }
            };
        }

        public static List<MaintenanceRecord> GetSampleMaintenance(List<Guid> shipIds)
        {
            if (shipIds.Count < 2) return new List<MaintenanceRecord>();

            return new List<MaintenanceRecord>
            {
                // Deliberately overdue record for the first ship
                new MaintenanceRecord
                {
                    ShipId = shipIds[0],
                    Type = MaintenanceType.Routine,
                    Description = "Main engine cylinder overhaul (10,000 hrs)",
                    ScheduledDate = DateTime.UtcNow.AddDays(-45),
                    EstimatedCost = 45000,
                    PerformedBy = "Wärtsilä Services",
                    Status = MaintenanceStatus.Overdue // Deliberately bypassing public graph rules
                },
                new MaintenanceRecord
                {
                    ShipId = shipIds[0],
                    Type = MaintenanceType.Regulatory,
                    Description = "Annual safety equipment inspection",
                    ScheduledDate = DateTime.UtcNow.AddDays(5),
                    EstimatedCost = 2500,
                    PerformedBy = "Class Society Inspector",
                    Status = MaintenanceStatus.Scheduled
                },
                new MaintenanceRecord
                {
                    ShipId = shipIds[1],
                    Type = MaintenanceType.Emergency,
                    Description = "Auxiliary generator #2 turbocharger repair",
                    ScheduledDate = DateTime.UtcNow.AddDays(-2),
                    EstimatedCost = 12500,
                    PerformedBy = "Ship Crew",
                    Status = MaintenanceStatus.InProgress
                },
                new MaintenanceRecord
                {
                    ShipId = shipIds[1],
                    Type = MaintenanceType.Scheduled,
                    Description = "Hull cleaning and propeller polishing",
                    ScheduledDate = DateTime.UtcNow.AddDays(-30),
                    CompletedDate = DateTime.UtcNow.AddDays(-28),
                    EstimatedCost = 8000,
                    ActualCost = 8250,
                    PerformedBy = "Subsea Global",
                    Status = MaintenanceStatus.Completed
                }
            };
        }

        public static List<FuelLog> GetSampleFuelLogs(List<Guid> shipIds, List<Guid> voyageIds)
        {
            if (shipIds.Count < 2) return new List<FuelLog>();

            var logs = new List<FuelLog>();
            
            // Baseline logs for ship 0
            for (int i = 0; i < 3; i++)
            {
                logs.Add(new FuelLog
                {
                    ShipId = shipIds[0],
                    VoyageId = voyageIds.Count > i ? voyageIds[i] : null,
                    FuelType = FuelType.LowSulfurFuelOil,
                    QuantityLiters = 150000,
                    CostPerLiter = 0.8500m, // Baseline ~0.85/L
                    RecordedDate = DateTime.UtcNow.AddDays(-60 + (i * 15)),
                    Notes = $"Routine bunkering operation {i+1}"
                });
            }

            // Anomalous log for ship 0
            logs.Add(new FuelLog
            {
                ShipId = shipIds[0],
                VoyageId = voyageIds.Count > 3 ? voyageIds[3] : null,
                FuelType = FuelType.LowSulfurFuelOil,
                QuantityLiters = 50000,
                CostPerLiter = 1.3500m, // Significantly higher than 0.85 (by >50%)
                RecordedDate = DateTime.UtcNow.AddDays(-2),
                Notes = "Emergency bunkering at remote port - premium price paid"
            });

            // Normal logs for ship 1
            logs.Add(new FuelLog
            {
                ShipId = shipIds[1],
                VoyageId = null,
                FuelType = FuelType.HeavyFuelOil,
                QuantityLiters = 250000,
                CostPerLiter = 0.6200m,
                RecordedDate = DateTime.UtcNow.AddDays(-10),
                Notes = "Full bunkering at home port"
            });

            return logs;
        }
    }
}
