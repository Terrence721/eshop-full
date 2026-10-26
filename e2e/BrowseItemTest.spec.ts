import { test, expect } from '@playwright/test';

test('Browse Items', async ({ page }) => {
  await page.goto('/catalog');

  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible();

  await page.getByText('Adventurer GPS Watch').click();

  //Expect
  await expect(page.getByRole('heading', { name: 'Adventurer GPS Watch' })).toBeVisible();
});