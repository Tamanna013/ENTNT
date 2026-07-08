using System;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models;
using FleetMind.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FleetMind.Api.Data.Seed
{
    public class DatabaseSeeder
    {
        private readonly FleetMindDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            FleetMindDbContext context,
            IPasswordHasher passwordHasher,
            IConfiguration configuration,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }

        private async Task SeedRolesAsync()
        {
            var roleNames = RoleSeedData.GetAllRoleNames();
            
            foreach (var roleName in roleNames)
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.Name.ToLower() == roleName.ToLower());
                if (!roleExists)
                {
                    _context.Roles.Add(new Role
                    {
                        Name = roleName
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            var adminEmail = _configuration["SeedAdmin:Email"];
            var adminPassword = _configuration["SeedAdmin:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                _logger.LogWarning("SeedAdmin configuration is missing or incomplete. Skipping admin seeding.");
                return;
            }

            var adminExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == adminEmail.ToLower());
            if (adminExists)
            {
                return; // Idempotent
            }

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.Admin);
            if (adminRole == null)
            {
                _logger.LogError("Admin role does not exist, cannot seed admin user.");
                return;
            }

            var adminUser = new User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = adminEmail,
                PasswordHash = _passwordHasher.Hash(adminPassword),
                IsActive = true,
                IsEmailVerified = true
            };

            _context.Users.Add(adminUser);
            
            _context.UserRoles.Add(new UserRole
            {
                User = adminUser,
                RoleId = adminRole.Id
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded development admin account: {email} - this account is for LOCAL DEVELOPMENT ONLY and must not be relied upon in staging or production environments.", adminEmail);
        }

        public async Task SeedSampleDataAsync()
        {
            // Idempotent check
            var sampleFleetNames = SampleDataSeedData.SampleFleets.Select(f => f.Name).ToList();
            var anySampleFleetExists = await _context.Fleets.AnyAsync(f => sampleFleetNames.Contains(f.Name));
            
            if (anySampleFleetExists)
            {
                _logger.LogInformation("Sample fleets already exist. Skipping sample data seeding.");
                return;
            }

            // Seed Fleets
            foreach (var fleet in SampleDataSeedData.SampleFleets)
            {
                _context.Fleets.Add(fleet);
            }
            await _context.SaveChangesAsync();

            // Seed Ships
            var fleetIds = SampleDataSeedData.SampleFleets.Select(f => f.Id).ToList();
            var ships = SampleDataSeedData.GetSampleShips(fleetIds);
            
            foreach (var ship in ships)
            {
                _context.Ships.Add(ship);
            }
            await _context.SaveChangesAsync();

            // Seed Crew
            var shipIds = ships.Select(s => s.Id).ToList();
            var crewMembers = SampleDataSeedData.GetSampleCrewMembers(shipIds);
            
            foreach (var crew in crewMembers)
            {
                _context.CrewMembers.Add(crew);
            }
            await _context.SaveChangesAsync();

            // Seed sample certifications for dashboard (Requires dummy Attachment records)
            // Note: These attachments do not exist on disk, their DownloadUrl will return 404
            var assignedCrew = crewMembers.Where(c => c.ShipId.HasValue).Take(3).ToList();
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == _configuration["SeedAdmin:Email"]);
            var adminId = adminUser?.Id ?? Guid.Empty;

            if (assignedCrew.Count >= 3)
            {
                var attachments = new List<Attachment>
                {
                    new Attachment { EntityName = AttachmentEntityType.Crew, EntityId = assignedCrew[0].Id, FileName = "STCW_Basic.pdf", ContentType = "application/pdf", FileSizeBytes = 1024, StoredFileName = "dummy1.pdf", UploadedByUserId = adminId },
                    new Attachment { EntityName = AttachmentEntityType.Crew, EntityId = assignedCrew[1].Id, FileName = "Medical_Clearance.pdf", ContentType = "application/pdf", FileSizeBytes = 2048, StoredFileName = "dummy2.pdf", UploadedByUserId = adminId },
                    new Attachment { EntityName = AttachmentEntityType.Crew, EntityId = assignedCrew[2].Id, FileName = "Advanced_Firefighting.pdf", ContentType = "application/pdf", FileSizeBytes = 512, StoredFileName = "dummy3.pdf", UploadedByUserId = adminId }
                };

                _context.Attachments.AddRange(attachments);
                await _context.SaveChangesAsync();

                var certs = new List<CrewCertification>
                {
                    new CrewCertification { CrewMemberId = assignedCrew[0].Id, AttachmentId = attachments[0].Id, CertificationName = "STCW Basic Safety", ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)) }, // Expiring soon
                    new CrewCertification { CrewMemberId = assignedCrew[1].Id, AttachmentId = attachments[1].Id, CertificationName = "Medical Clearance", ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)) },  // Already expired
                    new CrewCertification { CrewMemberId = assignedCrew[2].Id, AttachmentId = attachments[2].Id, CertificationName = "Advanced Firefighting", ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(100)) } // Safe
                };

                _context.Set<CrewCertification>().AddRange(certs);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Successfully seeded sample data for Fleets, Ships, and Crew.");
            
            await SeedPhase3SampleDataAsync();
        }

        private async Task SeedPhase3SampleDataAsync()
        {
            var sampleVoyageNumber = "PAC-2026-0417";
            var alreadySeeded = await _context.Voyages.AnyAsync(v => v.VoyageNumber == sampleVoyageNumber);
            if (alreadySeeded)
            {
                _logger.LogInformation("Phase 3 sample data already exists. Skipping.");
                return;
            }

            // Get Ships to link Voyages
            var ships = await _context.Ships.Take(8).ToListAsync();
            if (ships.Count == 0) return;

            var shipIds = ships.Select(s => s.Id).ToList();
            
            var voyages = Phase3SampleDataSeedData.GetSampleVoyages(shipIds);
            _context.Voyages.AddRange(voyages);
            await _context.SaveChangesAsync();

            var voyageIds = voyages.Select(v => v.Id).ToList();

            var cargos = Phase3SampleDataSeedData.GetSampleCargo(voyageIds);
            _context.Cargo.AddRange(cargos);
            await _context.SaveChangesAsync();

            var containers = Phase3SampleDataSeedData.GetSampleContainers(voyageIds);
            _context.Containers.AddRange(containers);
            await _context.SaveChangesAsync();

            // Link Cargo to Containers
            // Find Cargo for voyage 2 and container for voyage 2
            var cargoForVoyage2 = cargos.FirstOrDefault(c => c.VoyageId == voyageIds[2]);
            var containerForVoyage2 = containers.FirstOrDefault(c => c.CurrentVoyageId == voyageIds[2]);
            if (cargoForVoyage2 != null && containerForVoyage2 != null)
            {
                _context.Set<ContainerCargoItem>().Add(new ContainerCargoItem
                {
                    ContainerId = containerForVoyage2.Id,
                    CargoId = cargoForVoyage2.Id
                });
            }

            // Find Cargo for voyage 0 and container for voyage 0
            var cargoForVoyage0 = cargos.FirstOrDefault(c => c.VoyageId == voyageIds[0] && c.Type == CargoType.Hazardous);
            var containerForVoyage0 = containers.FirstOrDefault(c => c.CurrentVoyageId == voyageIds[0]);
            if (cargoForVoyage0 != null && containerForVoyage0 != null)
            {
                _context.Set<ContainerCargoItem>().Add(new ContainerCargoItem
                {
                    ContainerId = containerForVoyage0.Id,
                    CargoId = cargoForVoyage0.Id
                });
            }
            await _context.SaveChangesAsync();

            var containerIds = containers.Select(c => c.Id).ToList();
            var trackingEvents = Phase3SampleDataSeedData.GetSampleTrackingEvents(containerIds);
            _context.Set<ContainerTrackingEvent>().AddRange(trackingEvents);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded sample data for Voyages, Cargo, and Containers.");

            await SeedPhase4SampleDataAsync();
        }

        private async Task SeedPhase4SampleDataAsync()
        {
            var laPortUnLocode = "USLAX";
            var alreadySeeded = await _context.Ports.AnyAsync(p => p.UnLocode == laPortUnLocode);
            if (alreadySeeded)
            {
                _logger.LogInformation("Phase 4 sample data already exists. Skipping.");
                return;
            }

            // 1. Seed Ports
            var ports = Phase4SampleDataSeedData.GetSamplePorts();
            _context.Ports.AddRange(ports);
            await _context.SaveChangesAsync();

            var uslax = ports.First(p => p.UnLocode == "USLAX");
            var nltrm = ports.First(p => p.UnLocode == "NLRTM");
            var cnsha = ports.First(p => p.UnLocode == "CNSHA");

            // 2. Clean up backfilled data
            // Pointing placeholder HomePortId/OriginPortId/DestinationPortId to real ports
            var fleets = await _context.Fleets.ToListAsync();
            foreach (var fleet in fleets)
            {
                // Explicitly assigning real port to first couple fleets
                fleet.HomePortId = uslax.Id;
            }
            _context.Fleets.UpdateRange(fleets);

            var voyages = await _context.Voyages.ToListAsync();
            foreach (var voyage in voyages)
            {
                voyage.OriginPortId = cnsha.Id;
                voyage.DestinationPortId = uslax.Id;
            }
            _context.Voyages.UpdateRange(voyages);
            await _context.SaveChangesAsync();

            // 3. Seed Maintenance
            var ships = await _context.Ships.Take(8).ToListAsync();
            if (ships.Count == 0) return;

            var shipIds = ships.Select(s => s.Id).ToList();
            var maintenanceRecords = Phase4SampleDataSeedData.GetSampleMaintenance(shipIds);
            _context.MaintenanceRecords.AddRange(maintenanceRecords);
            await _context.SaveChangesAsync();

            // 4. Seed Fuel Logs
            var voyageIds = voyages.Select(v => v.Id).ToList();
            var fuelLogs = Phase4SampleDataSeedData.GetSampleFuelLogs(shipIds, voyageIds);
            _context.FuelLogs.AddRange(fuelLogs);
            await _context.SaveChangesAsync();

            // 5. Create notifications for the seeded anomalous and overdue records
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == _configuration["SeedAdmin:Email"]);
            if (adminUser != null)
            {
                var overdueRecord = maintenanceRecords.FirstOrDefault(r => r.Status == MaintenanceStatus.Overdue);
                if (overdueRecord != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = adminUser.Id,
                        Type = NotificationType.MaintenanceOverdue,
                        Title = "Maintenance Overdue",
                        Message = $"Maintenance '{overdueRecord.Description}' is now overdue.",
                        RelatedEntityName = "MaintenanceRecord",
                        RelatedEntityId = overdueRecord.Id,
                        IsRead = false
                    });
                }

                var anomalousFuelLog = fuelLogs.FirstOrDefault(l => l.CostPerLiter > 1.00m);
                if (anomalousFuelLog != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = adminUser.Id,
                        Type = NotificationType.FuelAnomaly,
                        Title = "Fuel Cost Anomaly",
                        Message = $"Fuel log has a cost per liter of {anomalousFuelLog.CostPerLiter:C} which exceeds the 90-day average by >50%.",
                        RelatedEntityName = "FuelLog",
                        RelatedEntityId = anomalousFuelLog.Id,
                        IsRead = false
                    });
                }
                
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Successfully seeded sample data for Ports, Maintenance, Fuel, and Notifications.");

            await SeedPhase5SampleDataAsync();
        }

        private async Task SeedPhase5SampleDataAsync()
        {
            var alreadySeeded = await _context.Incidents.AnyAsync();
            if (alreadySeeded)
            {
                _logger.LogInformation("Phase 5 sample data already exists. Skipping.");
                return;
            }

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == _configuration["SeedAdmin:Email"]);
            if (adminUser == null) return;

            var ships = await _context.Ships.Take(5).ToListAsync();
            var shipIds = ships.Select(s => s.Id).ToList();

            var voyages = await _context.Voyages.Take(5).ToListAsync();
            var voyageIds = voyages.Select(v => v.Id).ToList();

            var incidents = Phase5SampleDataSeedData.GetSampleIncidents(shipIds, voyageIds, adminUser.Id);
            _context.Incidents.AddRange(incidents);
            await _context.SaveChangesAsync();

            var (documents, versions, attachments) = Phase5SampleDataSeedData.GetSampleDocuments(adminUser.Id, shipIds);
            _context.Documents.AddRange(documents);
            _context.Attachments.AddRange(attachments);
            _context.Set<DocumentVersion>().AddRange(versions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded sample data for Incidents and Documents.");
        }
    }
}
