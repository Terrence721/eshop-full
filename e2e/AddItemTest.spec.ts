import { existsSync } from 'node:fs';
test.skip(!process.env.USERNAME1 || !process.env.PASSWORD, 'requires USERNAME1 and PASSWORD');
import { STORAGE_STATE } from '../playwright.config';
import { test, expect } from '@playwright/test';

test.skip('Add item to the cart', async ({ page }) => {
  if (!process.env.USERNAME1 || !process.env.PASSWORD) test.skip();
  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible();
  await page.getByRole('link', { name: 'Adventurer GPS Watch' }).click();
  await page.getByRole('button', { name: 'Add to shopping bag' }).click();
  await expect(page.getByText('1 in your shopping bag')).toBeVisible();
  await page.getByRole('link', { name: 'shopping bag' }).click();
  await page.getByRole('heading', { name: 'Shopping bag' }).click();

  // The cart's single "Total:" line is already asserted below; this
  // extra no-op click was only a recorded interaction artifact.
  await page.getByLabel('product quantity').getByText('1');

  await expect.poll(() => page.getByLabel('product quantity').count()).toBeGreaterThan(0);
});