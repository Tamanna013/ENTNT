using System;
using System.Collections.Generic;
using FleetMind.Api.Models;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Data.Seed
{
    public static class Phase3SampleDataSeedData
    {
        public static List<Voyage> GetSampleVoyages(List<Guid> shipIds)
        {
            var now = DateTime.UtcNow;

            return new List<Voyage>
            {
                // Deliberately overdue "Scheduled" voyage for Milestone 48 test
                new Voyage
                {
                    VoyageNumber = "PAC-2026-0417",
                    ShipId = shipIds[0],
                    // OriginPortId = portIds[0],
                    // DestinationPortId = portIds[1],
                    DepartureDate = now.AddDays(-5),
                    EstimatedArrivalDate = now.AddHours(-3),
                    Status = VoyageStatus.Scheduled
                },
                // Another overdue "Scheduled" voyage
                new Voyage
                {
                    VoyageNumber = "PAC-2026-0418",
                    ShipId = shipIds[1],
                    // OriginPortId = portIds[2],
                    // DestinationPortId = portIds[3],
                    DepartureDate = now.AddDays(-6),
                    EstimatedArrivalDate = now.AddHours(-1),
                    Status = VoyageStatus.Scheduled
                },
                // "InTransit" voyage
                new Voyage
                {
                    VoyageNumber = "ATL-2026-0101",
                    ShipId = shipIds[3],
                    // OriginPortId = portIds[4],
                    // DestinationPortId = portIds[5],
                    DepartureDate = now.AddDays(-2),
                    EstimatedArrivalDate = now.AddDays(5),
                    Status = VoyageStatus.InTransit
                },
                // "Completed" voyage
                new Voyage
                {
                    VoyageNumber = "NOR-2026-0222",
                    ShipId = shipIds[6],
                    // OriginPortId = portIds[6],
                    // DestinationPortId = portIds[7],
                    DepartureDate = now.AddDays(-10),
                    EstimatedArrivalDate = now.AddDays(-2),
                    ActualArrivalDate = now.AddDays(-2),
                    Status = VoyageStatus.Completed
                },
                // "Cancelled" voyage
                new Voyage
                {
                    VoyageNumber = "PAC-2026-9999",
                    ShipId = shipIds[2],
                    // OriginPortId = portIds[8],
                    // DestinationPortId = portIds[9],
                    DepartureDate = now.AddDays(10),
                    EstimatedArrivalDate = now.AddDays(20),
                    Status = VoyageStatus.Cancelled
                }
            };
        }

        public static List<Cargo> GetSampleCargo(List<Guid> voyageIds)
        {
            return new List<Cargo>
            {
                // Cargo for Voyage 0
                new Cargo { VoyageId = voyageIds[0], Description = "Electronics and Consumer Goods", Type = CargoType.GeneralGoods, Status = CargoStatus.Pending, WeightKg = 15000, DeclaredValue = 250000, ConsigneeName = "Tech Imports LLC" },
                new Cargo { VoyageId = voyageIds[0], Description = "Lithium Batteries", Type = CargoType.Hazardous, Status = CargoStatus.Pending, WeightKg = 5000, DeclaredValue = 100000, ConsigneeName = "PowerCell Inc", HazardNotes = "UN3480 Lithium ion batteries. Class 9." },
                // Cargo for Voyage 1
                new Cargo { VoyageId = voyageIds[1], Description = "Apparel", Type = CargoType.GeneralGoods, Status = CargoStatus.Pending, WeightKg = 8000, DeclaredValue = 120000, ConsigneeName = "Fashion Retailers" },
                // Cargo for Voyage 2
                new Cargo { VoyageId = voyageIds[2], Description = "Industrial Machinery", Type = CargoType.GeneralGoods, Status = CargoStatus.InTransit, WeightKg = 35000, DeclaredValue = 850000, ConsigneeName = "Heavy Duty Corp" },
                new Cargo { VoyageId = voyageIds[2], Description = "Chemical Precursors", Type = CargoType.Hazardous, Status = CargoStatus.InTransit, WeightKg = 12000, DeclaredValue = 400000, ConsigneeName = "ChemWorks", HazardNotes = "Corrosive. Keep away from water." },
                // Cargo for Voyage 3
                new Cargo { VoyageId = voyageIds[3], Description = "Frozen Seafood", Type = CargoType.Perishable, Status = CargoStatus.Delivered, WeightKg = 22000, DeclaredValue = 300000, ConsigneeName = "Nordic Foods" },
                new Cargo { VoyageId = voyageIds[3], Description = "Auto Parts", Type = CargoType.GeneralGoods, Status = CargoStatus.Delivered, WeightKg = 18000, DeclaredValue = 210000, ConsigneeName = "Oslo Auto" },
                // Cargo for Voyage 4
                new Cargo { VoyageId = voyageIds[4], Description = "Luxury Vehicles", Type = CargoType.Vehicles, Status = CargoStatus.Pending, WeightKg = 45000, DeclaredValue = 1500000, ConsigneeName = "West Coast Auto Group" }
            };
        }

        public static List<Container> GetSampleContainers(List<Guid> voyageIds)
        {
            return new List<Container>
            {
                new Container { ContainerNumber = "MSKU1234567", Status = ContainerStatus.InTransit, CurrentVoyageId = voyageIds[2] },
                new Container { ContainerNumber = "CMAU7654321", Status = ContainerStatus.Empty, CurrentVoyageId = null },
                new Container { ContainerNumber = "HLXU9998887", Status = ContainerStatus.InTransit, CurrentVoyageId = voyageIds[0] },
                new Container { ContainerNumber = "TCNU1112223", Status = ContainerStatus.Empty, CurrentVoyageId = null }
            };
        }

        public static List<ContainerTrackingEvent> GetSampleTrackingEvents(List<Guid> containerIds)
        {
            var now = DateTime.UtcNow;
            return new List<ContainerTrackingEvent>
            {
                // Deliberately non-sequential insertion for Container 0
                new ContainerTrackingEvent { ContainerId = containerIds[0], EventType = "InTransit", Location = "Mid-Atlantic", Timestamp = now.AddDays(-1), Notes = "Routine check" },
                new ContainerTrackingEvent { ContainerId = containerIds[0], EventType = "Loaded", Location = "Rotterdam", Timestamp = now.AddDays(-2), Notes = "Loaded onto vessel" },
                
                // For Container 2
                new ContainerTrackingEvent { ContainerId = containerIds[2], EventType = "Loaded", Location = "Shanghai", Timestamp = now.AddDays(-5), Notes = "Sealed and loaded" }
            };
        }
    }
}
