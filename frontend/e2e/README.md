# End-to-End Testing (Playwright)

This directory contains the Playwright end-to-end tests for FleetMind AI.

## Prerequisites

1. **Both Backend and Frontend MUST be running locally**:
   - Start the backend API (e.g. `dotnet run` from `backend/FleetMind.Api` or via Visual Studio/Rider).
   - Start the frontend dev server (`npm run dev` from the `frontend` folder). It should run on `http://localhost:5173`.
2. **Database State**: The tests rely on connecting directly to the LocalDB `FleetMindDb`. Ensure your database is migrated and healthy. This test flow generates unique user accounts on the fly to avoid data collision.
3. **One-Time Setup**:
   - Ensure you have installed the npm dependencies (`npm install`).
   - Install the required Playwright browser binaries by running:
     ```bash
     npx playwright install
     ```

## Running the Tests

To execute the test suite:

```bash
npm run test:e2e
```

## Note on Email Verification

During the registration test flow, the test script directly connects to the SQL Server database using the `mssql` library to query the `EmailVerificationTokens` table for the newly generated user's token.

**Why?**
Parsing the live console logs of the backend API for the "mock email sent" link is extremely fragile in automated environments (buffering, timing, concurrent output). Directly querying the database provides a deterministic and robust way to complete the email verification step. This choice prioritizes test stability while still guaranteeing the actual verification API endpoint behaves correctly.
