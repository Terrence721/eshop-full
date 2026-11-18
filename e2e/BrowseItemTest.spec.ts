import { test, expect } from '@playwright/test';

test('Browse Items', async ({ page }) => {
  await page.goto('/catalog');

  // Wait for page to be fully loaded
  await page.waitForLoadState('networkidle');
  // Wait for the main heading to appear (increase timeout for SPA load)
  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible({ timeout: 60000 });

  await page.getByText('Adventurer GPS Watch').click();

  // Expect product detail heading
  await expect(page.getByRole('heading', { name: 'Adventurer GPS Watch' })).toBeVisible({ timeout: 60000 });
});