import { test, expect } from '@playwright/test';

test('ClientApp browser head smoke test', async ({ page }) => {
  await page.goto('/');

  // The Avalonia browser head renders the same splash / canvas surface.
  await expect(page.locator('#out')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible();

  // Placeholder: deeper interactions are Avalonia-specific and not drivable from Playwright.
});