import { test, expect } from '@playwright/test';

test.describe('WebApp homepage hotspots', () => {
  test('shows the homepage image only on the landing page', async ({ page }) => {
    await page.goto('/');
    // Ensure page fully loaded
    await page.waitForLoadState('networkidle');

    await expect(page.getByAltText('eShop homepage')).toBeVisible({ timeout: 30000 });
    // No catalog grid or filters are rendered on the landing page any more.
    await expect(page.locator('.catalog-grid')).toHaveCount(0);
    await expect(page.locator('.catalog-filters')).toHaveCount(0);
  });

  test('All hotspot opens the brand-sorted catalog', async ({ page }) => {
    await page.goto('/');
    // Ensure page fully loaded
    await page.waitForLoadState('networkidle');

    await page.getByRole('button', { name: 'All catalog items', exact: true }).click({ timeout: 30000 });
    await expect(page).toHaveURL(/\/catalog$/, { timeout: 30000 });

    // "All" now also shows every product (200 per page in the client).
    await expect(page.locator('.catalog-item-card').first()).toBeVisible({ timeout: 30000 });
  });

  test('AirStrider hotspot opens the AirStrider-filtered catalog', async ({ page }) => {
    await page.goto('/');
    // Ensure page fully loaded
    await page.waitForLoadState('networkidle');

    await page.getByRole('button', { name: 'AirStrider catalog items', exact: true }).click({ timeout: 30000 });
    await expect(page).toHaveURL(/\/catalog\?brand=9$/, { timeout: 30000 });
  });

  test('person avatar opens a demo login modal and shows success', async ({ page }) => {
    await page.goto('/');
    // Ensure page fully loaded
    await page.waitForLoadState('networkidle');
    // Avatar hotspot is the Log in affordance.
    await page.getByRole('button', { name: 'Log in', exact: true }).first().click({ timeout: 30000 });
    await expect(page.getByRole('dialog')).toBeVisible({ timeout: 30000 });

    await page.getByPlaceholder('Username').fill('alice');
    await page.getByPlaceholder('Password').fill('Pass123$');
    await page.getByRole('dialog').getByRole('button', { name: 'Log in' }).click({ timeout: 30000 });

    await expect(page.getByText('Login successful')).toBeVisible({ timeout: 30000 });
    await expect(page.getByText(/Welcome,\s*alice!/)).toBeVisible({ timeout: 30000 });
  });

  test('header no longer shows shopping bag or log-in text', async ({ page }) => {
    await page.goto('/');
    // Ensure page fully loaded
    await page.waitForLoadState('networkidle');

    await expect(page.getByRole('link', { name: 'shopping bag' })).toHaveCount(0);
    await expect(page.getByRole('link', { name: 'Log in' })).toHaveCount(0);
  });
});