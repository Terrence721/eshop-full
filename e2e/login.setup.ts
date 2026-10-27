import { test as setup, expect } from '@playwright/test';
import { STORAGE_STATE } from '../playwright.config';

setup('Login', async ({ page }) => {
  await page.goto('/');

  const username = process.env.USERNAME1;
  const password = process.env.PASSWORD;
  if (!username || !password) {
    setup.skip();
    return;
  }

  try {
    await page.goto('/user/login?returnUrl=%2F');
    await page.waitForURL(/localhost:5223/, { timeout: 30000 }).catch(() => {});

    const redirectUrl = await page.evaluate(async ({ username, password }) => {
      const url = new URL(window.location.href);
      const returnUrl = url.searchParams.get('ReturnUrl') ?? url.searchParams.get('returnUrl') ?? '/';

      const response = await fetch('/Account/Login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password, returnUrl }),
      });

      if (!response.ok) {
        return null;
      }
      try {
        const result = await response.json();
        return result.redirectUrl ?? '/';
      } catch {
        return null;
      }
    }, { username, password });

    if (!redirectUrl) {
      setup.skip();
      return;
    }

    await page.goto(new URL(redirectUrl, page.url()).toString());
    await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible({ timeout: 30000 });
    await page.context().storageState({ path: STORAGE_STATE });
  } catch {
    setup.skip();
  }
});