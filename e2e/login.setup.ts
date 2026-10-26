import { test as setup, expect } from '@playwright/test';
import { STORAGE_STATE } from '../playwright.config';

setup('Login', async ({ page }) => {
  await page.goto('/');

  const username = process.env.USERNAME1;
  const password = process.env.PASSWORD;
  if (!username || !password) {
    throw new Error('USERNAME1/PASSWORD environment variables are required');
  }

  await page.goto('/user/login?returnUrl=%2F');

  const redirectUrl = await page.evaluate(async ({ username, password }) => {
    const url = new URL(window.location.href);
    const returnUrl = url.searchParams.get('ReturnUrl') ?? url.searchParams.get('returnUrl') ?? '/';

    const response = await fetch('/Account/Login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password, returnUrl }),
    });

    const result = await response.json();
    return result.redirectUrl ?? '/';
  }, { username, password });

  await page.evaluate((url) => {
    window.location.href = url;
  }, redirectUrl);
  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible({ timeout: 30000 });
  await page.context().storageState({ path: STORAGE_STATE });
});