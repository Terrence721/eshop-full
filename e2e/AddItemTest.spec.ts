import { test, expect } from '@playwright/test';

test('Add item to the cart', async ({ page }) => {
  if (!process.env.USERNAME1 || !process.env.PASSWORD) test.skip();
  await page.goto('/catalog');

  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible();
  await page.getByRole('link', { name: 'Adventurer GPS Watch' }).click();
  // The handler returns immediately, so waiting for a matching text node
  // would pass before the basket mutation finishes. Wait for the actual PUT
  // to the BFF instead, then the derived count is guaranteed.
  const basketSave = page.waitForResponse((response) =>
    response.url().includes('/api/basket') && response.request().method() === 'PUT',
  );
  await page.getByRole('button', { name: 'Add to shopping bag' }).click();
  await basketSave;
  await expect(page.getByText(/in your shopping bag/)).toBeVisible();
  await page.goto('/cart');
  await expect(page.getByRole('heading', { name: 'Shopping bag' })).toBeVisible();

  await expect(page.getByLabel('Quantity for Adventurer GPS Watch')).toBeVisible();
});