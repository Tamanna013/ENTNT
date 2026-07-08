# Analytics Caching Strategy

This document outlines the caching approach and policies for the Analytics layer built during Phase 6. Because aggregate analytics are read-heavy, computationally expensive, and tolerate slight staleness, all endpoint outputs are intentionally cached using an in-memory layer (`ICacheService`) governed by configuration (`AnalyticsExpirationMinutes`, defaulting to 15 minutes).

## Cache Keys & Expiration

The system uses the following cache keys dynamically appended with the requested time window (e.g. `months=12`). This ensures different requests cleanly miss each other and don't mistakenly serve differing windows from the same cache.

| Endpoint | Cache Key Pattern | Expiration Policy |
| :--- | :--- | :--- |
| **Fleet Summary** | `analytics:fleet-summary` | `CacheOptions.AnalyticsExpirationMinutes` |
| **Ship Utilization** | `analytics:ship-utilization-trend:{months}` | `CacheOptions.AnalyticsExpirationMinutes` |
| **Voyage Performance** | `analytics:voyage-performance:{months}` | `CacheOptions.AnalyticsExpirationMinutes` |
| **Crew Compliance** | `analytics:crew-compliance:{months}` | `CacheOptions.AnalyticsExpirationMinutes` |
| **Maintenance Cost** | `analytics:maintenance-cost-trend:{months}` | `CacheOptions.AnalyticsExpirationMinutes` |
| **Financial Summary** | `analytics:financial-summary:{months}` | `CacheOptions.AnalyticsExpirationMinutes` |

## Cache Warmup

The `AnalyticsCacheWarmupService` is a one-shot `IHostedService` executing shortly after startup. It programmatically hits every single analytics endpoint using their default parameter (`months = 12`), wrapping them individually in `try/catch` scopes.

This deliberate startup action proactively populates the default timeframe variants of the caching layer, preventing the very first dashboard visitor from having to pay the heavy combined computation cost.

## Invalidation Strategy

**We deliberately do NOT implement proactive cache invalidation across the system.**

### The Reasoning
The natural cache expiration window (defaulting to 15 minutes) is already appropriately short relative to how slowly aggregate, retrospective trend metrics meaningfully shift.

Implementing a proactive invalidation architecture (e.g., triggering a purge to `analytics:ship-utilization-trend` every time a `MaintenanceRecord` is closed) would demand wiring complex, cross-domain dependencies into every core write path. Specifically, `Ship`, `Maintenance`, `Voyage`, `CrewCertification`, and `FuelLog` domains would suddenly need explicit coupling to caching artifacts that have nothing to do with their domain concerns.

A user seeing data that is up to 15 minutes stale is an entirely acceptable tradeoff when counterbalanced against the architectural complexity of implementing and maintaining a sprawling invalidation web. 

This decision may be revisited if a real-time analytics requirement is explicitly raised by stakeholders in the future, but currently, it is neither necessary nor justified.
