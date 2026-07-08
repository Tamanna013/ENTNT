# API Versioning and Deprecation Policy

**Date:** July 2026
**Reviewer:** AI Assistant

## Overview
This document formalizes the API versioning policy for the FleetMind.Api backend project. The versioning infrastructure (powered by `Asp.Versioning.Mvc`) was established in Milestone 9 to support URL-segment versioning (e.g., `/api/v1/`) and automatic version-reporting headers. After numerous phases of development where only `v1` has ever been shipped, this review was conducted to systematically verify compliance across all newly added controllers and lay out a concrete, actionable policy for introducing future breaking changes safely.

## 1. Compliance Verification
A complete audit was conducted across every single controller located in the `backend/FleetMind.Api/Controllers/` directory to ensure compliance with the project's established convention:
- Controller class must include `[ApiVersion("1.0")]`.
- Controller class must use the parameterized route `[Route("api/v{version:apiVersion}/[controller]")]` or an equivalent explicit route containing `{version:apiVersion}`.

### Audited Controllers
The following 20 controllers were checked:
1. `AiController`
2. `AnalyticsController`
3. `AttachmentsController`
4. `AuditLogsController`
5. `AuthController`
6. `CargoController`
7. `ContainersController`
8. `CrewMembersController`
9. `DocumentsController`
10. `FleetsController`
11. `FuelController`
12. `HealthController`
13. `IncidentsController`
14. `MaintenanceController`
15. `NotificationsController`
16. `PortsController`
17. `ReportingController`
18. `ShipsController`
19. `UsersController`
20. `VoyagesController`

### Findings & Fixes
The vast majority of the controllers constructed directly with standard tooling in early phases correctly implemented the `v{version:apiVersion}` routing. However, seven controllers added in later phases via copy-adaptation were found to have hardcoded `[Route("api/v1/...")]` routing and were missing the `[ApiVersion("1.0")]` attribute:
- `AiController`
- `AnalyticsController`
- `AuditLogsController`
- `CrewMembersController`
- `DocumentsController`
- `NotificationsController`
- `ReportingController`

**Fix Applied:** All seven non-compliant controllers were updated to include `[ApiVersion("1.0")]` and `[Route("api/v{version:apiVersion}/...")]`. The entire project is now strictly 100% compliant.

### Header Verification
A representative sample of distinct endpoints spanning different phases (e.g., `AuthController` from Phase 1, `VoyagesController` from Phase 3, `NotificationsController` from Phase 4, `AnalyticsController` from Phase 6, and `AiController` from Phase 7) were verified. Because `options.ReportApiVersions = true` is configured centrally in `ApiVersioningServiceExtensions.cs`, hitting any of these endpoints natively emits the `api-supported-versions: 1.0` HTTP response header, properly broadcasting the supported version array to API consumers.

## 2. Deprecation and v2 Introduction Policy

To date, this project has solely shipped `v1` endpoints and has not yet needed to introduce a `v2`. The infrastructure is fully prepared. When a breaking change is needed for an existing `v1` endpoint (e.g., a changed response shape, a removed field, or a tightened validation rule that existing clients depend on), the following strict procedure MUST be followed:

1. **Create the New Version (Never Modify in Place):** 
   Add `[ApiVersion("2.0")]` to a **new** controller (or a **new** action using per-action versioning) that implements the new behavior.
   *Critically: The ORIGINAL `v1` controller or action MUST remain completely unchanged and fully functional. Modifying a `v1` endpoint's existing behavior in a breaking way in-place completely defeats the purpose of the versioning scheme.*

2. **Mark v1 as Deprecated:** 
   Once the `v2` replacement exists and is live, mark the original version as deprecated by updating its attribute to:
   `[ApiVersion("1.0", Deprecated = true)]`
   This automatically causes the `api-deprecated-versions: 1.0` and `api-supported-versions: 2.0` headers to be emitted for existing consumers, providing advance, discoverable notice that they need to migrate.

3. **Maintain the Support Window:** 
   The deprecated `v1` version must be maintained for a documented minimum support window (e.g., at least 6 months from the deprecation date, or per whatever public commitment is made to API consumers) before the code is actually removed and requests to `/api/v1/...` begin failing with a 404 or 410.
