# SQL Injection & Raw Query Audit

**Date:** July 2026
**Reviewer:** AI Assistant

## Overview
This document records a systematic codebase-wide audit of all raw SQL touchpoints in the FleetMind.Api backend project. The primary goal of this review was to verify that the parameterization discipline established in early milestones was followed consistently across the entire application, eliminating the risk of SQL injection vulnerabilities.

## 1. Audit Methodology
The audit consisted of the following comprehensive checks across the entire backend `.cs` codebase:
1. **Search for `FromSqlRaw`**: Searched for any usage of `FromSqlRaw` to identify potentially unsafe string concatenations directly passed into EF Core.
2. **Search for `FromSqlInterpolated`**: Searched for all usages of `FromSqlInterpolated` to manually verify that the C# string interpolation (`$"..."`) correctly encapsulates strongly-typed variables without prior string concatenation.
3. **Search for Raw ADO.NET (`SqlConnection` / `SqlCommand`)**: Verified if raw database connections were executing dynamic, unparameterized query text outside the purview of EF Core.
4. **Spot-Check EF Core LINQ Generation**: Reviewed representative repositories (e.g., `FleetRepository`) to ensure dynamic `WHERE` clauses are constructed using strongly-typed lambda expressions (`.Where(x => ...)`) rather than string-based Dynamic LINQ predicates.

## 2. Findings and Verification

### A. `FromSqlRaw` Usage
- **Findings:** Zero instances found.
- **Status:** **PASS**. The risk of raw string concatenation being directly executed via `FromSqlRaw` is entirely mitigated because the method is not used anywhere in the codebase.

### B. `FromSqlInterpolated` Usage
- **Findings:** Exactly three call sites were found, all located in `backend/FleetMind.Api/Repositories/ReportingRepository.cs`:
  1. `.FromSqlInterpolated($"EXEC sp_GetFleetUtilizationReport")`
  2. `.FromSqlInterpolated($"EXEC sp_GetVoyageManifestReport @VoyageId = {voyageId}")`
  3. `.FromSqlInterpolated($"EXEC sp_GetFuelEfficiencyReport @TrailingDays = {trailingDays}")`
- **Verification:** **PASS**. All three queries are constructed correctly. `voyageId` and `trailingDays` are passed as native C# variables inside the interpolated string, which EF Core natively converts into `DbParameter` instances (e.g., `@p0`, `@p1`). No variables are pre-concatenated into strings before interpolation.

### C. Raw ADO.NET Usage (`SqlConnection` / `SqlCommand`)
- **Findings for `SqlCommand`:** Zero instances found.
- **Findings for `SqlConnection`:** Only found in `backend/FleetMind.Api/Controllers/HealthController.cs`.
- **Verification:** **PASS**. The `HealthController` opens a raw connection (`await connection.OpenAsync();`) purely to test database connectivity for load-balancer probes. No query text is ever executed on this connection, entirely eliminating any SQL injection attack surface.

### D. EF Core LINQ Predicates
- **Findings:** A spot-check of `FleetRepository.cs` and other standard repositories confirmed that dynamic filtering is handled exclusively via chained LINQ predicates (e.g., `queryable.Where(f => f.Status == query.Status)`).
- **Verification:** **PASS**. EF Core's LINQ provider natively parameterizes these expressions. There is no usage of unsafe Dynamic LINQ string building.

## 3. Checklist for Future Contributors
To maintain this secure posture, all future development MUST adhere to the following rules:
- **Never use `FromSqlRaw`** with concatenated strings.
- **Always use `FromSqlInterpolated`** with proper `$"..."` syntax for any raw SQL queries or stored procedure calls, even for simple, seemingly safe parameter types like `Guid` or `int`.
- **Rely on standard LINQ lambda expressions** for dynamic `WHERE` filtering instead of string-based predicates.
