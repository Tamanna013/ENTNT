using System.Text;
using FleetMind.Api.Configuration;
using FleetMind.Api.Data;
using FleetMind.Api.Extensions;
using FleetMind.Api.Middleware;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

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
            retainedFileCountLimit: 30));

    // ─── Services ────────────────────────────────────────────────────────────

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    });

    // Database options (connection string via options pattern)
    builder.Services.Configure<DatabaseOptions>(
        builder.Configuration.GetSection(DatabaseOptions.SectionName));

    // Entity Framework Core
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
    builder.Services.Configure<JwtOptions>(
        builder.Configuration.GetSection(JwtOptions.SectionName));

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
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

    builder.Services.AddAuthorization();

    // Application Services
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // CORS — allow the React dev server during development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

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
}
finally
{
    Log.CloseAndFlush();
}
