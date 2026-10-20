import { test as setup, expect } from '@playwright/test';
import { STORAGE_STATE } from '../playwright.config';

setup('Login', async ({ page }) => {
  await page.goto('/');

  await page.getByRole('link', { name: 'Log in' }).click();
  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible({ timeout: 30000 });

  await page.context().storageState({ path: STORAGE_STATE });
});