# Query Performance & Configuration Review

This document covers two foundational configuration and query performance concerns addressed in Milestone 133: SQL Server connection pooling and EF Core `AsNoTracking()` consistency.

## 1. Connection Pool Sizing

### Current State
Previously, our `appsettings.json` and `appsettings.Development.json` files relied on SQL Server’s implicit connection pooling defaults (typically Min Pool Size = 0, Max Pool Size = 100 for `Microsoft.Data.SqlClient`). While functionally sufficient, implicit defaults obscure the active database configuration and delay connection instantiation during burst loads after idle periods.

### Change Applied
We updated the connection strings to explicitly configure pool sizes:
- `Min Pool Size=5`: Maintains a small subset of warm connections. This prevents the connection-establishment latency penalty on the first few requests following an idle period.
- `Max Pool Size=100`: Explicitly caps the pool at 100 connections. While 100 is often the implicit default, making it explicit acts as living documentation, providing developers immediate visibility into the API's concurrency bounds without looking up provider defaults.

## 2. `AsNoTracking()` Re-verification Sweep

### The Principle
- **Read-Only Methods**: Methods used strictly for fetching data to display (e.g., `GetPagedAsync`) *must* use `.AsNoTracking()` to avoid unnecessary EF Core change-tracking overhead.
- **Fetch-Then-Modify Methods**: Methods whose output is later modified and saved via `_unitOfWork.SaveChangesAsync()` (e.g., `GetByIdAsync`, `GetByEmailAsync`) *must not* use `.AsNoTracking()`. Otherwise, the entity changes are silently ignored by the change tracker.

### Repositories & Methods Verified
We performed a systematic sweep across the following repositories checking `GetPagedAsync`, `GetByIdAsync`, `FindAsync`, `GetAllAsync`, and specialized query methods.

#### Read-Only Methods (Confirmed Correct & Consistent)
The following explicitly read-only methods were confirmed to successfully implement `.AsNoTracking()`:
- `AuditLogRepository.GetPagedAsync`
- `CargoRepository.GetPagedAsync`
- `ContainerRepository.GetPagedAsync`
- `CrewMemberRepository.GetPagedAsync`
- `DocumentRepository.GetPagedAsync`
- `FleetRepository.GetPagedAsync`
- `FuelLogRepository.GetPagedAsync`
- `IncidentRepository.GetPagedAsync`
- `MaintenanceRecordRepository.GetPagedAsync`
- `PortRepository.GetPagedAsync`
- `ShipRepository.GetPagedAsync`
- `UserRepository.GetPagedAsync`
- `VoyageRepository.GetPagedAsync`

#### Fetch-Then-Modify Methods (Confirmed Correct)
The following specialized methods were verified to intentionally exclude `.AsNoTracking()`, as they are utilized by `Update*Async` service routines:
- `ShipRepository.GetByIdWithFleetAsync` (used by `ShipService.UpdateShipAsync`)
- `VoyageRepository.GetByIdWithShipAsync` (used by `VoyageService.UpdateVoyageAsync` and `CompleteVoyageAsync`)
- `RefreshTokenRepository.GetByTokenHashAsync`

### Gaps Found and Remedied
During the audit, we identified and corrected two critical gaps where `.AsNoTracking()` was improperly applied to fetch-then-modify paths:

1. **`GenericRepository<T>`**
   - **Issue**: The base methods `GetByIdAsync`, `GetAllAsync`, and `FindAsync` unconditionally applied `.AsNoTracking()`. Since these methods are invoked generically across the entire API by virtually every `UpdateXAsync` service method, applying `.AsNoTracking()` here bypassed standard change-tracking.
   - **Fix**: Removed `.AsNoTracking()` from these methods.
2. **`UserRepository`**
   - **Issue**: `GetByEmailAsync` applied `.AsNoTracking()`. However, `AuthService.LoginAsync` uses this method to fetch a user, increment `FailedLoginAttempts`, and possibly set `LockedOutUntil`, before calling `SaveChangesAsync()`. These security updates were silently failing because the entity was untracked.
   - **Fix**: Removed `.AsNoTracking()` from `UserRepository.GetByEmailAsync`.

## Conclusion
Connection pooling is now explicit, and EF Core change-tracking correctly differentiates between pure-read performance paths and stateful fetch-then-modify business logic paths.
