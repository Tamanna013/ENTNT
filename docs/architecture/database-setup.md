# Database Setup Guide

This document explains how to set up a local SQL Server database for FleetMind AI development.

## Prerequisites

You need **one** of the following SQL Server options installed locally:

| Option | Best For | Download |
|--------|----------|----------|
| **SQL Server LocalDB** | Lightweight dev on Windows (ships with Visual Studio) | [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) |
| **SQL Server Developer Edition** | Full-featured free dev instance | [SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| **Docker (mssql/server)** | Cross-platform, isolated | See Docker section below |

## Option 1 — SQL Server LocalDB (Windows)

LocalDB is typically already installed with Visual Studio. Verify:

```bash
sqllocaldb info
```

Create the database:

```bash
sqllocaldb start mssqllocaldb
sqlcmd -S "(localdb)\mssqllocaldb" -Q "CREATE DATABASE FleetMindDb;"
```

Connection string for `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FleetMindDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Option 2 — SQL Server Developer Edition (Windows)

After installing SQL Server Developer Edition:

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
2. Connect to your local instance (usually `localhost` or `.\SQLEXPRESS`)
3. Create a new database named `FleetMindDb`

Connection string for `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FleetMindDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

## Option 3 — Docker (Cross-Platform)

Pull and run the SQL Server container:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name fleetmind-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Create the database:

```bash
docker exec -it fleetmind-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U SA -P "YourStrong@Passw0rd" -C \
  -Q "CREATE DATABASE FleetMindDb;"
```

Connection string for `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=FleetMindDb;User Id=SA;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

## Setting the Connection String

Edit `backend/FleetMind.Api/appsettings.Development.json` with the connection string matching your setup above.

> **⚠️ Important:** `appsettings.Development.json` is **gitignored** and must never be committed.
> The checked-in `appsettings.json` contains only a placeholder value (`REPLACE_IN_LOCAL_SETTINGS`).

For production deployments, connection strings should be provided via:
- **Azure App Service** → Connection Strings configuration
- **Azure Key Vault** → Referenced via Key Vault provider
- **Environment variables** → `ConnectionStrings__DefaultConnection`

## Running Migrations

### Installing the EF Core CLI Tool

The project uses a **local tool manifest** so every developer gets the same `dotnet-ef` version. From the `backend/` directory:

```bash
cd backend
dotnet tool restore          # installs dotnet-ef from the manifest
```

If you need to install it for the first time (the manifest already exists in `.config/dotnet-tools.json`):

```bash
dotnet tool install dotnet-ef --version 9.0.15
```

### Creating a New Migration

```bash
cd backend
dotnet ef migrations add <MigrationName> --project FleetMind.Api
```

### Applying Migrations to the Database

```bash
cd backend
dotnet ef database update --project FleetMind.Api
```

### Rolling Back a Migration

```bash
cd backend
dotnet ef database update <PreviousMigrationName> --project FleetMind.Api
```

### Removing the Last Unapplied Migration

```bash
cd backend
dotnet ef migrations remove --project FleetMind.Api
```

> **💡 Tip:** Always review the generated migration files in `FleetMind.Api/Migrations/`
> before applying them. Verify they contain only expected schema changes and no sensitive data.

## Verifying Connectivity


After setting up your database and connection string, start the API and test:

```bash
cd backend
dotnet run --project FleetMind.Api

# In another terminal:
curl http://localhost:5000/api/v1/health/db
```

Expected response:

```json
{
  "status": "ok",
  "database": "connected"
}
```

If the database is unreachable, the endpoint returns HTTP 503 with error details.
