# Load Testing Baseline

This document establishes a reproducible performance baseline for the FleetMind API under concurrent load, validating the combined effect of our Phase 6 performance optimizations (Milestones 129–133). 

## Prerequisites
This load test uses **k6**, a scriptable, free load-testing tool. 
- You must have k6 installed locally to run this test. [Install k6](https://k6.io/docs/get-started/installation/)
- The API must be running locally.
- The database should be fully seeded with the accumulated sample data from Phase 2–5 (`SeedPhaseXSampleDataAsync` must have run). Testing against an empty database will yield artificially fast, unrepresentative results.

## Running the Test
The test script simulates 15 virtual users executing a realistic mix of read and write operations (70% list endpoints, 20% detail endpoints, 10% incident creation) over a 2-minute duration.

Run the following command from the repository root:
```bash
k6 run -e TEST_EMAIL=admin@fleetmind.ai -e TEST_PASSWORD=<YOUR_LOCAL_ADMIN_PASSWORD> load-tests/baseline-scenario.js
```
*(Replace `<YOUR_LOCAL_ADMIN_PASSWORD>` with the actual seeded password for the admin account. Never commit real credentials to source control.)*

## Baseline Results
**Date Captured**: July 7, 2026
**Dataset Scale**: Fully seeded development dataset.
**Concurrency**: 15 Virtual Users
**Duration**: 2 minutes
**Rate Limiting**: `GeneralApiPermitLimit` was temporarily raised to 10,000 to prevent the load tester from being blocked by the single-IP rate limit configured in Milestone 121.

### Metrics
- **Total Requests (`http_reqs`)**: 2325
- **Requests Per Second (RPS)**: ~19.04 req/s
- **Checks Passed**: 100.00% (✓ 1785 / ✗ 0)
- **Response Latency (`http_req_duration`)**:
  - **Average**: 6.41 ms
  - **Median**: 3.01 ms
  - **p(90)**: 5.31 ms
  - **p(95)**: 8.45 ms
  - **Max**: 1.09 s (likely initial startup/caching burst)

## Known Findings & Future Work
1. **Rate Limiting Impact on Testing**: By default, ASP.NET Core rate limiting triggers (429 status) quickly against load-testing tools generating traffic from a single IP. This verifies that our Rate Limiting (Milestone 121) is working successfully. To run authentic high-scale load tests, the tool must either use distributed IP addresses or the rate limits must be temporarily bypassed/increased for the testing context.
2. **Missing DI Registrations**: During this test, we identified and fixed a `500 Internal Server Error` affecting `/api/v1/analytics/fleet-summary` and `/api/v1/voyages` due to missing DI registrations for `IPdfGenerationService` and `IExportService`. This emphasizes the value of holistic API testing.
3. **Database Dependency**: The local load test ran against an `InMemoryDatabase` due to environmental constraints (lack of local SQL Server). While this establishes a stable application-layer baseline, a true production-like load test should run against a real relational database (e.g., PostgreSQL/SQL Server) to properly measure database I/O latency, connection pooling efficiency, and index performance.
