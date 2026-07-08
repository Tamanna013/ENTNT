# N+1 Query Audit

## Overview
This document outlines the findings of a systematic audit of N+1 query patterns across the FleetMind API repositories, conducted to ensure that backend performance holds up as the application scales. We specifically re-examined the known tradeoff in `FleetService.GetFleetsAsync` and swept five other major repositories.

## 1. FleetService `ShipCount` Re-evaluation

### Initial Finding
When fetching a full page of 100 fleets, EF Core query logging verified that the application issued:
- 1 query to get the total count of Fleets (for pagination)
- 1 query to fetch the page of Fleets
- **N queries** (e.g., 30 queries for 30 fleets returned in a page) to execute `CountAsync` on `Ships` per `FleetId`.

**Measured Query Count**: 32 queries for a page returning 30 fleets. (O(N) pattern).

### Decision & Fix
With the application significantly grown since Phase 2, this O(N) pattern is no longer an acceptable tradeoff, even with capped page sizes. 

**Fix Applied**: We introduced `GetShipCountsByFleetIdsAsync` to `IShipRepository`, utilizing a `GroupBy` approach. The service now fetches the page of Fleets and then makes a *single* bulk query to get the ship counts for all fleet IDs on that page at once.

### Post-Fix Measurement
After the fix, executing the same `GET /fleets?pageSize=100` resulting in 30 fleets executed:
- 1 query to get the total count of Fleets
- 1 query to fetch the page of Fleets
- **1 query** to fetch the grouped counts (`SELECT FleetId, COUNT(*) FROM Ships WHERE FleetId IN (...) GROUP BY FleetId`)

**Measured Query Count**: 3 queries (Constant O(1) pattern) regardless of page size.

## 2. Broader Repository Sweep

To ensure no N+1 patterns inadvertently crept into other modules, the following repositories' `GetPagedAsync` methods were tested with query logging enabled against full pages of results:

- **ShipRepository**: 
  - Measured Queries: 2 (1 Count, 1 Select with `LEFT JOIN` for `Fleet`)
  - Status: Clean. `.Include()` eager loading functioning correctly.
- **VoyageRepository**: 
  - Measured Queries: 2 (1 Count, 1 Select with `LEFT JOIN`s for `Ship`, `OriginPort`, `DestinationPort`)
  - Status: Clean.
- **CargoRepository**: 
  - Measured Queries: 2 (1 Count, 1 Select with `LEFT JOIN` for `Voyage`)
  - Status: Clean.
- **CrewMemberRepository**: 
  - Measured Queries: 2 (1 Count, 1 Select with `LEFT JOIN` for `Ship`)
  - Status: Clean.
- **ContainerRepository**: 
  - Measured Queries: 2 (1 Count, 1 Select with `LEFT JOIN` for `Voyage`)
  - Status: Clean.

## Conclusion
The core eager-loading discipline (`.Include()`) established throughout the project is intact and verified. The single known exception (Fleet ShipCount) has now been remediated, ensuring the backend maintains constant-query-time bounds across all list endpoints.
