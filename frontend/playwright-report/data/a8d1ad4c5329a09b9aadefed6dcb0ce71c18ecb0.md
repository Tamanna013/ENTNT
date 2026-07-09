# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: full-user-journey.spec.ts >> Full User Journey >> Registration to Logout connected workflow
- Location: e2e\full-user-journey.spec.ts:14:3

# Error details

```
Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5173/register
Call log:
  - navigating to "http://localhost:5173/register", waiting until "load"

```

# Test source

```ts
  1   | import { test, expect } from '@playwright/test';
  2   | import { execSync } from 'child_process';
  3   | import path from 'path';
  4   | 
  5   | test.describe('Full User Journey', () => {
  6   |   // Generate a unique email for each run to avoid data collisions
  7   |   const uniqueId = Date.now();
  8   |   const testEmail = `playwright.test.${uniqueId}@fleetmind.ai`;
  9   |   const testPassword = 'TestPassword123!';
  10  |   const fleetName = `E2E Fleet ${uniqueId}`;
  11  |   const shipName = `E2E Ship ${uniqueId}`;
  12  |   const shipImo = `9${String(uniqueId).substring(5, 12).padEnd(6, '0')}`; // Must be exactly 7 digits starting with IMO, but validation just takes 7 digits for IMO field
  13  | 
  14  |   test('Registration to Logout connected workflow', async ({ page }) => {
  15  |     // 1. Navigate to Registration Page
> 16  |     await page.goto('/register');
      |                ^ Error: page.goto: net::ERR_CONNECTION_REFUSED at http://localhost:5173/register
  17  |     await expect(page).toHaveURL(/.*\/register/);
  18  | 
  19  |     // 2. Fill out Registration Form
  20  |     await page.fill('input[name="firstName"]', 'Test');
  21  |     await page.fill('input[name="lastName"]', 'User');
  22  |     await page.fill('input[name="email"]', testEmail);
  23  |     await page.fill('input[name="password"]', testPassword);
  24  |     await page.fill('input[name="confirmPassword"]', testPassword);
  25  |     
  26  |     // Submit
  27  |     await page.click('button[type="submit"]');
  28  | 
  29  |     // Confirm success message/UI appears
  30  |     await expect(page.locator('text=Registration successful')).toBeVisible();
  31  | 
  32  |     // 3. Extract Email Verification Token from Database
  33  |     // We run the DbHelper C# console app built specifically for fetching the token
  34  |     // This avoids fragile log-parsing in automated tests.
  35  |     const dbHelperPath = path.resolve(__dirname, '../../backend/DbHelper');
  36  |     let token = '';
  37  |     
  38  |     // Give the backend a small moment to write the token to DB (auto-wait concept implemented via retry for DB access)
  39  |     await expect(async () => {
  40  |       const output = execSync(`dotnet run --project "${dbHelperPath}" ${testEmail}`, { encoding: 'utf-8' });
  41  |       token = output.trim();
  42  |       expect(token).not.toBe('TOKEN_NOT_FOUND');
  43  |       expect(token.length).toBeGreaterThan(10);
  44  |     }).toPass({ timeout: 10000 }); // Retries until successful or 10s timeout
  45  | 
  46  |     // 4. Verify Email using the extracted token
  47  |     await page.goto(`/verify-email?token=${token}`);
  48  |     
  49  |     // Verify we get redirected to login, or a success message is shown
  50  |     await expect(page).toHaveURL(/.*\/login/);
  51  | 
  52  |     // 5. Log in with new credentials
  53  |     await page.fill('input[name="email"]', testEmail);
  54  |     await page.fill('input[name="password"]', testPassword);
  55  |     await page.click('button[type="submit"]');
  56  | 
  57  |     // Confirm successful redirection to Dashboard
  58  |     await expect(page).toHaveURL(/.*\/dashboard/);
  59  |     await expect(page.locator('h1:has-text("Dashboard")')).toBeVisible();
  60  | 
  61  |     // 6. Navigate to Fleets & Create Fleet
  62  |     await page.click('nav a[href="/fleets"]');
  63  |     await expect(page).toHaveURL(/.*\/fleets/);
  64  |     
  65  |     await page.click('button:has-text("Create Fleet")');
  66  |     await expect(page.locator('h2:has-text("Create Fleet")')).toBeVisible();
  67  |     
  68  |     await page.fill('input[name="name"]', fleetName);
  69  |     await page.fill('textarea[name="description"]', 'E2E test fleet description');
  70  |     
  71  |     // Select port from the dropdown (wait for it to load)
  72  |     await page.waitForSelector('select[name="homePortId"] option[value]');
  73  |     const selectPort = await page.locator('select[name="homePortId"]');
  74  |     const firstPortOption = await selectPort.locator('option').nth(1).getAttribute('value');
  75  |     if (firstPortOption) {
  76  |         await selectPort.selectOption(firstPortOption);
  77  |     }
  78  |     
  79  |     await page.selectOption('select[name="status"]', 'Active');
  80  |     
  81  |     await page.click('button[type="submit"]:has-text("Save Fleet")');
  82  | 
  83  |     // Confirm fleet appears in the table
  84  |     await expect(page.locator(`text=${fleetName}`)).toBeVisible();
  85  | 
  86  |     // 7. Navigate into Fleet Detail Page & Create Ship
  87  |     await page.click(`text=${fleetName}`);
  88  |     
  89  |     // Ensure we reached the fleet detail page
  90  |     await expect(page.locator(`h1:has-text("${fleetName}")`)).toBeVisible();
  91  |     
  92  |     // Navigate to Ships from the detail or via nav?
  93  |     // The application might have a "Ships" tab or just clicking on ships nav
  94  |     await page.click('nav a[href="/ships"]');
  95  |     await expect(page).toHaveURL(/.*\/ships/);
  96  | 
  97  |     await page.click('button:has-text("Add Ship")');
  98  |     
  99  |     await page.fill('input[name="name"]', shipName);
  100 |     await page.fill('input[name="imoNumber"]', shipImo);
  101 |     await page.selectOption('select[name="type"]', 'Container');
  102 |     await page.selectOption('select[name="status"]', 'Active');
  103 |     await page.fill('input[name="capacityTeq"]', '1000');
  104 |     
  105 |     // Select the fleet we just created
  106 |     await page.waitForSelector('select[name="fleetId"] option[value]');
  107 |     await page.selectOption('select[name="fleetId"]', { label: fleetName });
  108 | 
  109 |     await page.click('button[type="submit"]:has-text("Save Ship")');
  110 | 
  111 |     // Confirm ship appears in the list
  112 |     await expect(page.locator(`text=${shipName}`)).toBeVisible();
  113 | 
  114 |     // 8. Navigate to Ship Detail Page
  115 |     await page.click(`text=${shipName}`);
  116 |     
```