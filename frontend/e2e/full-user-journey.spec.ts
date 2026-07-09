import { test, expect } from '@playwright/test';
import { execSync } from 'child_process';
import path from 'path';

test.describe('Full User Journey', () => {
  // Generate a unique email for each run to avoid data collisions
  const uniqueId = Date.now();
  const testEmail = `playwright.test.${uniqueId}@fleetmind.ai`;
  const testPassword = 'TestPassword123!';
  const fleetName = `E2E Fleet ${uniqueId}`;
  const shipName = `E2E Ship ${uniqueId}`;
  const shipImo = `9${String(uniqueId).substring(5, 12).padEnd(6, '0')}`; // Must be exactly 7 digits starting with IMO, but validation just takes 7 digits for IMO field

  test('Registration to Logout connected workflow', async ({ page }) => {
    // 1. Navigate to Registration Page
    await page.goto('/register');
    await expect(page).toHaveURL(/.*\/register/);

    // 2. Fill out Registration Form
    await page.fill('input[name="firstName"]', 'Test');
    await page.fill('input[name="lastName"]', 'User');
    await page.fill('input[name="email"]', testEmail);
    await page.fill('input[name="password"]', testPassword);
    await page.fill('input[name="confirmPassword"]', testPassword);
    
    // Submit
    await page.click('button[type="submit"]');

    // Confirm success message/UI appears
    await expect(page.locator('text=Registration successful')).toBeVisible();

    // 3. Extract Email Verification Token from Database
    // We run the DbHelper C# console app built specifically for fetching the token
    // This avoids fragile log-parsing in automated tests.
    const dbHelperPath = path.resolve(__dirname, '../../backend/DbHelper');
    let token = '';
    
    // Give the backend a small moment to write the token to DB (auto-wait concept implemented via retry for DB access)
    await expect(async () => {
      const output = execSync(`dotnet run --project "${dbHelperPath}" ${testEmail}`, { encoding: 'utf-8' });
      token = output.trim();
      expect(token).not.toBe('TOKEN_NOT_FOUND');
      expect(token.length).toBeGreaterThan(10);
    }).toPass({ timeout: 10000 }); // Retries until successful or 10s timeout

    // 4. Verify Email using the extracted token
    await page.goto(`/verify-email?token=${token}`);
    
    // Verify we get redirected to login, or a success message is shown
    await expect(page).toHaveURL(/.*\/login/);

    // 5. Log in with new credentials
    await page.fill('input[name="email"]', testEmail);
    await page.fill('input[name="password"]', testPassword);
    await page.click('button[type="submit"]');

    // Confirm successful redirection to Dashboard
    await expect(page).toHaveURL(/.*\/dashboard/);
    await expect(page.locator('h1:has-text("Dashboard")')).toBeVisible();

    // 6. Navigate to Fleets & Create Fleet
    await page.click('nav a[href="/fleets"]');
    await expect(page).toHaveURL(/.*\/fleets/);
    
    await page.click('button:has-text("Create Fleet")');
    await expect(page.locator('h2:has-text("Create Fleet")')).toBeVisible();
    
    await page.fill('input[name="name"]', fleetName);
    await page.fill('textarea[name="description"]', 'E2E test fleet description');
    
    // Select port from the dropdown (wait for it to load)
    await page.waitForSelector('select[name="homePortId"] option[value]');
    const selectPort = await page.locator('select[name="homePortId"]');
    const firstPortOption = await selectPort.locator('option').nth(1).getAttribute('value');
    if (firstPortOption) {
        await selectPort.selectOption(firstPortOption);
    }
    
    await page.selectOption('select[name="status"]', 'Active');
    
    await page.click('button[type="submit"]:has-text("Save Fleet")');

    // Confirm fleet appears in the table
    await expect(page.locator(`text=${fleetName}`)).toBeVisible();

    // 7. Navigate into Fleet Detail Page & Create Ship
    await page.click(`text=${fleetName}`);
    
    // Ensure we reached the fleet detail page
    await expect(page.locator(`h1:has-text("${fleetName}")`)).toBeVisible();
    
    // Navigate to Ships from the detail or via nav?
    // The application might have a "Ships" tab or just clicking on ships nav
    await page.click('nav a[href="/ships"]');
    await expect(page).toHaveURL(/.*\/ships/);

    await page.click('button:has-text("Add Ship")');
    
    await page.fill('input[name="name"]', shipName);
    await page.fill('input[name="imoNumber"]', shipImo);
    await page.selectOption('select[name="type"]', 'Container');
    await page.selectOption('select[name="status"]', 'Active');
    await page.fill('input[name="capacityTeq"]', '1000');
    
    // Select the fleet we just created
    await page.waitForSelector('select[name="fleetId"] option[value]');
    await page.selectOption('select[name="fleetId"]', { label: fleetName });

    await page.click('button[type="submit"]:has-text("Save Ship")');

    // Confirm ship appears in the list
    await expect(page.locator(`text=${shipName}`)).toBeVisible();

    // 8. Navigate to Ship Detail Page
    await page.click(`text=${shipName}`);
    
    // Confirm Key Details
    await expect(page.locator(`h1:has-text("${shipName}")`)).toBeVisible();
    await expect(page.locator(`text=${shipImo}`)).toBeVisible();

    // 9. Log Out
    // Assuming Topbar has a user menu button that opens a dropdown with 'Logout'
    // or a direct 'Logout' button. Let's find a button that represents the user or says logout
    // FleetMind typically has an avatar or user name in the top right.
    await page.click('button[aria-label="User menu"]'); // common accessible pattern
    await page.click('button:has-text("Sign out")');
    
    // Confirm redirection to Login
    await expect(page).toHaveURL(/.*\/login/);
  });
});
