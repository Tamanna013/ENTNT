using System.Text;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Configuration;
using FleetMind.Api.Data;
using FleetMind.Api.Data.Seed;
using FleetMind.Api.Extensions;
using FleetMind.Api.Middleware;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FleetMind.Api.BackgroundServices;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Serilog;
using Microsoft.AspNetCore.ResponseCompression;

// ─── Serilog Bootstrap ───────────────────────────────────────────────────────
// Configure early so any startup errors are also captured.

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog((context, services, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/fleetmind-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30), 
        preserveStaticLogger: true);

    // ─── Services ────────────────────────────────────────────────────────────

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });

    /*
     * BREACH attack mitigation: Response compression has a known vulnerability class ("BREACH"),
     * which specifically targets compressed responses that reflect secret data back based on 
     * attacker-controlled input in the same response (e.g., a CSRF token in an HTML page).
     * Since this project is a pure JSON API with no HTML rendering and no endpoints that 
     * echo secret tokens alongside attacker-supplied content, it is not a meaningful target 
     * for this specific attack class. Thus, enabling compression globally is safe here.
     */
    builder.Services.AddResponseCompression(options => 
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
    });

    // Database options (connection string via options pattern)
    builder.Services.Configure<DatabaseOptions>(
        builder.Configuration.GetSection(DatabaseOptions.SectionName));

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<FleetMindDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Repositories & Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    // API Versioning
    builder.Services.AddFleetMindApiVersioning();

    // Swagger / OpenAPI with JWT Bearer support
    builder.Services.AddFleetMindSwagger();

    // AutoMapper — assembly scan for all Profile classes
    builder.Services.AddAutoMapper(typeof(Program).Assembly);

    // FluentValidation — assembly scan for all AbstractValidator<T> classes
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // JWT Authentication
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
    builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
    builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("Cache"));
    builder.Services.Configure<SecurityHeadersOptions>(builder.Configuration.GetSection("SecurityHeaders"));
    builder.Services.Configure<AccountLockoutOptions>(builder.Configuration.GetSection(AccountLockoutOptions.SectionName));

    // AI Provider Abstraction
    builder.Services.AddFleetMindAiProvider(builder.Configuration);

    builder.Services.Configure<BackgroundServiceOptions>(builder.Configuration.GetSection("BackgroundServices"));

    // Register Background Services
    builder.Services.AddHostedService<DelayedVoyageCheckService>();
    builder.Services.AddHostedService<MaintenanceOverdueCheckService>();
    builder.Services.AddHostedService<ExpiringCertificationCheckService>();
    builder.Services.AddHostedService<AnalyticsCacheWarmupService>();

    // Configure FormOptions to prevent Kestrel from rejecting large file uploads prematurely
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        // Set slightly higher than MaxFileSizeBytes to accommodate form overhead
        options.MultipartBodyLengthLimit = 10485760 + 1024;
    });

    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role // Explicitly tell ASP.NET where to find roles
        };
    });

    builder.Services.AddFleetMindAuthorization();
    builder.Services.AddFleetMindRateLimiting(builder.Configuration);

    // Application Services
    builder.Services.AddScoped<IFleetRepository, FleetRepository>();
    builder.Services.AddScoped<IShipRepository, ShipRepository>();
    builder.Services.AddScoped<IVoyageRepository, VoyageRepository>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddHostedService<DelayedVoyageCheckService>();
builder.Services.AddHostedService<MaintenanceOverdueCheckService>();
builder.Services.AddHostedService<ExpiringCertificationCheckService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<ICrewMemberRepository, CrewMemberRepository>();
    builder.Services.AddScoped<ICargoRepository, CargoRepository>();
    builder.Services.AddScoped<IContainerRepository, ContainerRepository>();
    builder.Services.AddScoped<IPortRepository, PortRepository>();
    builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
    builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<IFleetService, FleetService>();
    builder.Services.AddScoped<IShipService, ShipService>();
    builder.Services.AddScoped<ICrewMemberService, CrewMemberService>();
    builder.Services.AddScoped<IVoyageService, VoyageService>();
    builder.Services.AddScoped<ICargoService, CargoService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IContainerService, ContainerService>();
    builder.Services.AddScoped<IPortService, PortService>();
    builder.Services.AddScoped<IMaintenanceRecordService, MaintenanceRecordService>();
    builder.Services.AddScoped<IFuelLogService, FuelLogService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<INotificationRecipientResolver, NotificationRecipientResolver>();
    builder.Services.AddScoped<IReportingService, ReportingService>();
    builder.Services.AddScoped<IReportingRepository, ReportingRepository>();
    
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    builder.Services.AddScoped<INaturalLanguageSearchService, NaturalLanguageSearchService>();
    builder.Services.AddScoped<IAiConversationService, AiConversationService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
    builder.Services.AddScoped<IIncidentService, IncidentService>();
    builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
    builder.Services.AddScoped<IAttachmentService, AttachmentService>();
    builder.Services.AddScoped<DatabaseSeeder>();
    builder.Services.AddScoped<IEmailSender, MockEmailSender>();

    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();
    builder.Services.AddScoped<IExportService, ExportService>();

    // CORS — allow the React dev server during development, or deployed URL in production
    var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigin)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Application Insights integration (Optional, graceful degradation)
    var appInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
    {
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
    }

    var app = builder.Build();

    // Apply pending migrations automatically on startup so docker-compose up works seamlessly
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FleetMindDbContext>();
        db.Database.Migrate();
    }

    // Seed database in Development
    // Note: Production/staging seeding strategies (e.g. a dedicated admin-provisioning CLI command, 
    // or manual controlled SQL) are a deliberately separate concern to be addressed during the 
    // deployment phase, and this automatic seeding is intentionally gated to Development only.
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
        await seeder.SeedSampleDataAsync();
    }

    // ─── Startup Logging ─────────────────────────────────────────────────────

    var dbConnectionString = app.Configuration.GetConnectionString("DefaultConnection");
    var dbName = "unknown";
    try
    {
        var csBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(dbConnectionString);
        dbName = csBuilder.InitialCatalog;
    }
    catch { /* placeholder or malformed connection string — not fatal at startup */ }

    app.Logger.LogInformation("FleetMind AI API starting. Target database: {DatabaseName}", dbName);

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    // Exception handling MUST be the outermost middleware to catch everything.

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    
    // Security headers and rate limiting should be extremely early in the pipeline
    // before auth or controller execution.
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseRateLimiter();

    // Placed after security middleware but before auth to compress responses globally
    app.UseResponseCompression();

    // Serilog request logging (structured, replaces default ASP.NET request logging)
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "FleetMind AI API v1");
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
