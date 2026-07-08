# Configuration & Secrets Reference

**Date:** July 2026
**Reviewer:** AI Assistant

## Overview
This document consolidates every configuration value and secret utilized across the FleetMind application (accumulated over numerous development phases). It distinguishes explicitly between non-secret settings (which are safely committed to source control) and genuine secrets (which must be carefully injected via environment variables or secrets managers).

### Security Audit Findings
As part of Milestone 128, a comprehensive security audit was conducted on the entire Git history of `appsettings.json` using `git log -p`. 

**Findings:** The strict discipline established in Phase 1—"secrets only in gitignored files"—has been adhered to flawlessly. 
- The committed `appsettings.json` contains **zero** real secrets. 
- The `DefaultConnection` was properly established as a placeholder `"REPLACE_IN_LOCAL_SETTINGS"` from its very first commit. 
- API Keys, JWT Signing Keys, and Seed Admin Passwords have never touched the committed `.json` file at any point in the repository's history. 
- Both the backend `.gitignore` (ignoring `appsettings.Development.json`, `appsettings.Local.json`, `secrets.json`) and the frontend `.gitignore` (ignoring `.env`, `.env.local`, `.env.production`) correctly exclude sensitive configuration files.

---

## Backend Configuration (`FleetMind.Api`)

### Database
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `ConnectionStrings:DefaultConnection` | `appsettings.Development.json` | **Yes** | Connection string for SQL Server database (contains DB credentials). *Value lives ONLY in appsettings.Development.json (gitignored) for local development; a real deployed environment must supply this via environment variables or a proper secrets manager (e.g., Azure Key Vault), never a committed file - see the future deployment phase for the concrete mechanism.* |

### Authentication (`Jwt`)
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `Jwt:Issuer` | `appsettings.json` | No | Issuer claim (`iss`) for emitted JWTs. |
| `Jwt:Audience` | `appsettings.json` | No | Audience claim (`aud`) for emitted JWTs. |
| `Jwt:AccessTokenExpiryMinutes` | `appsettings.json` | No | Lifespan of a standard access token in minutes. |
| `Jwt:RefreshTokenExpiryDays` | `appsettings.json` | No | Lifespan of a refresh token in days. |
| `Jwt:SigningKey` | `appsettings.Development.json` | **Yes** | Cryptographic key used to sign and verify JWTs. *Value lives ONLY in appsettings.Development.json (gitignored) for local development; a real deployed environment must supply this via environment variables or a proper secrets manager (e.g., Azure Key Vault), never a committed file - see the future deployment phase for the concrete mechanism.* |

### Security & Rate Limiting
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `RateLimiting:AuthEndpointsPermitLimit` | `appsettings.json` | No | Maximum requests to authentication endpoints per window. |
| `RateLimiting:AuthEndpointsWindowMinutes` | `appsettings.json` | No | Time window for authentication rate limiting. |
| `RateLimiting:GeneralApiPermitLimit` | `appsettings.json` | No | Maximum requests to general API endpoints per window. |
| `RateLimiting:GeneralApiWindowMinutes` | `appsettings.json` | No | Time window for general API rate limiting. |
| `AccountLockout:MaxFailedAttempts` | `appsettings.json` | No | Failed login attempts before triggering an account lockout. |
| `AccountLockout:LockoutDurationMinutes` | `appsettings.json` | No | Duration an account remains locked after threshold is reached. |
| `SecurityHeaders:EnableHsts` | `appsettings.json` | No | Whether to emit the Strict-Transport-Security header (HSTS). |
| `SecurityHeaders:HstsMaxAgeDays` | `appsettings.json` | No | Duration the browser should enforce HTTPS via HSTS. |

### AI Integration (`AiProvider` & `AiRateLimit`)
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `AiProvider:Provider` | `appsettings.json` | No | Selection of AI backend (`None`, `AzureOpenAI`, `GoogleGemini`). |
| `AiProvider:ModelDeploymentName` | `appsettings.json` | No | The deployment/model name used for AI inference (e.g., `gpt-4o`). |
| `AiProvider:MaxTokens` | `appsettings.json` | No | Maximum tokens to generate in AI responses. |
| `AiProvider:TimeoutSeconds` | `appsettings.json` | No | Timeout for AI provider API calls. |
| `AiProvider:ApiKey` | `appsettings.Development.json` | **Yes** | API key for authenticating with the chosen AI provider. *Value lives ONLY in appsettings.Development.json (gitignored) for local development; a real deployed environment must supply this via environment variables or a proper secrets manager (e.g., Azure Key Vault), never a committed file - see the future deployment phase for the concrete mechanism.* |
| `AiProvider:Endpoint` | `appsettings.Development.json` | No | The base URL endpoint for Azure OpenAI (not needed for Gemini). Often environment-specific but not strictly a credential. |
| `AiRateLimit:MaxRequestsPerUserPerHour` | `appsettings.json` | No | Maximum number of AI queries a user can make per hour. |

### Storage & Caching
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `FileStorage:MaxFileSizeBytes` | `appsettings.json` | No | Maximum allowed size for uploaded files. |
| `FileStorage:AllowedExtensions` | `appsettings.json` | No | Allowed file extensions for attachments. |
| `FileStorage:LocalStoragePath` | `appsettings.json` | No | Directory path for storing uploaded files locally. |
| `Cache:DefaultAbsoluteExpirationMinutes` | `appsettings.json` | No | Default absolute expiration time for memory cache entries. |
| `Cache:AnalyticsExpirationMinutes` | `appsettings.json` | No | Expiration time for cached analytics aggregates. |

### Background Services
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `BackgroundServices:DelayedVoyageCheckIntervalMinutes` | `appsettings.json` | No | Polling interval for marking voyages as delayed. |
| `BackgroundServices:MaintenanceOverdueCheckIntervalMinutes` | `appsettings.json` | No | Polling interval for marking maintenance tasks as overdue. |
| `BackgroundServices:ExpiringCertificationCheckIntervalMinutes`| `appsettings.json` | No | Polling interval for checking crew certification expirations. |

### Database Seeding (Development Only)
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `SeedAdmin:Email` | `appsettings.Development.json` | No | Initial admin account email for local seeding. |
| `SeedAdmin:Password` | `appsettings.Development.json` | **Yes** | Initial admin account password for local seeding. *Value lives ONLY in appsettings.Development.json (gitignored) for local development; a real deployed environment must supply this via environment variables or a proper secrets manager (e.g., Azure Key Vault), never a committed file - see the future deployment phase for the concrete mechanism.* |

### Infrastructure
| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `AllowedHosts` | `appsettings.json` | No | Restricts the hosts the application will serve requests for. |
| `Logging:LogLevel` | `appsettings.json` | No | Minimum log levels for ASP.NET Core default logger. |
| `Serilog:MinimumLevel` | `appsettings.json` | No | Structured logging configuration and log levels. |

---

## Frontend Configuration (`frontend`)

| Setting Key | Location | Secret? | Purpose |
|-------------|----------|---------|---------|
| `VITE_API_BASE_URL` | `.env` | No | Base URL for backend API requests (e.g., `https://localhost:5001`). Note: While not a secret (as it runs in the user's browser anyway), it is specific to the deployment environment and is properly excluded via `.gitignore`, with only the empty `.env.example` committed. |
