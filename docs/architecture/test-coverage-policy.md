# Test Coverage Policy & CI Quality Gates

This document establishes the official test coverage baseline and CI gating thresholds for the FleetMind AI project.

## 1. Context & Prioritization Strategy

Our testing strategy, built systematically across multiple hardening and validation milestones, explicitly rejects the pursuit of arbitrary 100% test coverage targets. Instead, we have deliberately prioritized testing the **most business-rule-heavy, complex, and security-critical code**:

- Complex state machines (e.g., Voyage status transitions)
- Ownership validation and cross-tenant checks (e.g., Fuel Log to Ship relationships)
- Role-Based Access Control (RBAC) matrices and authorization boundary enforcement
- The end-to-end notification pipeline

We have intentionally excluded exhaustive test-padding of simple, pass-through CRUD service methods, boilerplate controllers, Entity Framework migrations, and auto-generated code. The thresholds defined below reflect **this deliberate prioritization**—they are grounded in real, measured coverage of our most critical paths, not a claim of comprehensive, line-by-line codebase coverage.

## 2. Actual Measured Coverage (Baseline)

As of Milestone 144, the true, measured coverage across our suites is as follows:

- **Frontend Component & Hook Tests (Vitest + v8)**
  - Statement Coverage: **73.98%**
  - Line Coverage: **76.54%**
  - *Note: Our frontend testing focused heavily on shared UI primitives, form validation components, protected routing (UX layer), and critical data-fetching hooks.*

- **Backend Unit Tests (Coverlet / XPlat Code Coverage)**
  - Line Coverage: **1.27%** (495 lines covered out of 38,829 valid lines)
  - *Note: This percentage appears numerically low because the measurement includes the entire backend assembly—including thousands of lines of EF Core Migrations, DTOs, and standard CRUD boilerplate—while our unit tests strictly target the few core domain services containing complex business logic.*

## 3. Calibrated CI Gate Thresholds

To prevent future test coverage regressions without introducing brittle pipelines that break on trivial changes, our CI gating thresholds are set slightly below our measured baselines.

### Frontend CI Threshold
- **Target:** **70%** Statement Coverage
- **Rationale:** Gives a ~4% safety buffer below the 73.98% baseline to accommodate minor UI refactoring or non-critical file additions without immediately failing the build.

### Backend CI Threshold
- **Target:** **1%** Line Coverage (Unit Tests)
- **Rationale:** Ensures that the existing highly-critical business logic tests are not deleted or bypassed, while acknowledging the vast amount of un-tested boilerplate and generated code. Integration tests serve as the primary end-to-end confidence measure for the API.

## 4. Enforcement

These thresholds will be automatically enforced during the CI pipeline execution (`dotnet test` and `npm run test:coverage`). Any pull request causing coverage to dip below these calibrated thresholds will be rejected, forcing the author to either add tests for their new business logic or explicitly justify a threshold adjustment in this policy document.
