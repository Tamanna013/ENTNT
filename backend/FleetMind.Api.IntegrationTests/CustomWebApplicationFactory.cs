using System;
using System.Data.Common;
using System.Linq;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Configuration;
using FleetMind.Api.Data;
using FleetMind.Api.IntegrationTests.TestHelpers;
using FleetMind.Api.Services;
using FleetMind.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
namespace FleetMind.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection _connection;

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "a-very-long-super-secret-key-for-testing-purposes-only-12345");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new System.Collections.Generic.Dictionary<string, string?>
            {
                { "Jwt:SigningKey", "a-very-long-super-secret-key-for-testing-purposes-only-12345" }
            };
            config.AddInMemoryCollection(testConfig);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<FleetMindDbContext>));
            services.RemoveAll(typeof(DbContextOptions));
            services.RemoveAll(typeof(DbConnection));
            // Create open SqliteConnection so in-memory database doesn't close
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton<DbContextOptions<FleetMindDbContext>>(sp =>
            {
                return new DbContextOptionsBuilder<FleetMindDbContext>()
                    .UseSqlite(_connection)
                    .UseApplicationServiceProvider(sp)
                    .Options;
            });
            
            // Bypass Rate Limiting for integration tests by setting massive limits
            services.Configure<RateLimitingOptions>(options =>
            {
                options.AuthEndpointsPermitLimit = 10000;
                options.GeneralApiPermitLimit = 10000;
            });
            
            // Mock IReportingService because SQLite doesn't support the stored procedures used by the real repository
            services.AddScoped<IReportingService, MockReportingService>();
            
            // Re-build service provider to seed the database
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<FleetMindDbContext>();
            var passwordHasher = scopedServices.GetRequiredService<FleetMind.Api.Services.IPasswordHasher>();

            db.Database.EnsureCreated();

            // Seed users
            if (!db.Users.Any())
            {
                var adminRole = new Role { Id = Guid.NewGuid(), Name = AppRoles.Admin };
                var userRole = new Role { Id = Guid.NewGuid(), Name = AppRoles.User };
                var fleetManagerRole = new Role { Id = Guid.NewGuid(), Name = AppRoles.FleetManager };
                var maintenanceOfficerRole = new Role { Id = Guid.NewGuid(), Name = AppRoles.MaintenanceOfficer };
                var crewManagerRole = new Role { Id = Guid.NewGuid(), Name = AppRoles.CrewManager };
                db.Roles.AddRange(adminRole, userRole, fleetManagerRole, maintenanceOfficerRole, crewManagerRole);
                db.SaveChanges();

                var adminUser = new User { Id = Guid.NewGuid(), Email = "admin@test.com", FirstName = "Admin", LastName = "Test", PasswordHash = passwordHasher.Hash("AdminPass123!") };
                var normalUser = new User { Id = Guid.NewGuid(), Email = "user@test.com", FirstName = "Plain", LastName = "User", PasswordHash = passwordHasher.Hash("UserPass123!") };
                var fleetManagerUser = new User { Id = Guid.NewGuid(), Email = "fleetmanager@test.com", FirstName = "Fleet", LastName = "Manager", PasswordHash = passwordHasher.Hash("FleetManagerPass123!") };
                var maintenanceUser = new User { Id = Guid.NewGuid(), Email = "maintenanceofficer@test.com", FirstName = "Maintenance", LastName = "Officer", PasswordHash = passwordHasher.Hash("MaintenancePass123!") };
                var crewManagerUser = new User { Id = Guid.NewGuid(), Email = "crewmanager@test.com", FirstName = "Crew", LastName = "Manager", PasswordHash = passwordHasher.Hash("CrewPass123!") };

                db.Users.AddRange(adminUser, normalUser, fleetManagerUser, maintenanceUser, crewManagerUser);
                db.SaveChanges();

                db.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
                db.UserRoles.Add(new UserRole { UserId = normalUser.Id, RoleId = userRole.Id });
                db.UserRoles.Add(new UserRole { UserId = fleetManagerUser.Id, RoleId = fleetManagerRole.Id });
                db.UserRoles.Add(new UserRole { UserId = maintenanceUser.Id, RoleId = maintenanceOfficerRole.Id });
                db.UserRoles.Add(new UserRole { UserId = crewManagerUser.Id, RoleId = crewManagerRole.Id });
                db.SaveChanges();

                var port = new Port 
                { 
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
                    Name = "Test Port", 
                    UnLocode = "TSTPT", 
                    Country = "Testland", 
                    Latitude = 0, 
                    Longitude = 0
                };
                db.Ports.Add(port);
                
                var fleet = new Fleet
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Name = "Test Fleet",
                    HomePortId = port.Id,
                    Status = FleetStatus.Active
                };
                db.Fleets.Add(fleet);

                var ship = new Ship
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Name = "Test Ship",
                    IMO = "1234567",
                    Type = "Container",
                    Flag = "PA",
                    YearBuilt = 2020,
                    GrossTonnage = 10000,
                    FleetId = fleet.Id,
                    Status = ShipStatus.Active
                };
                db.Ships.Add(ship);

                db.SaveChanges();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
        _connection?.Dispose();
    }
}
