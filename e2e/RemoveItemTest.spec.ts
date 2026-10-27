import { existsSync } from 'node:fs';
import { STORAGE_STATE } from '../playwright.config';
import { test, expect } from '@playwright/test';

test('Remove item from cart', async ({ page }) => {
  if (!process.env.USERNAME1 || !process.env.PASSWORD) test.skip();
  await page.goto('/catalog');
  await expect(page.getByRole('heading', { name: 'eShop' })).toBeVisible();
  
  await page.getByRole('link', { name: 'Adventurer GPS Watch' }).click();
  await expect(page.getByRole('heading', { name: 'Adventurer GPS Watch' })).toBeVisible();
  
  await page.getByRole('button', { name: 'Add to shopping bag' }).click();
  await page.goto('/cart');
  await expect(page.getByRole('heading', { name: 'Shopping bag' })).toBeVisible();

  await expect.poll(() => page.getByLabel('Quantity for Adventurer GPS Watch').count()).toBeGreaterThan(0);
  
  await page.getByLabel('Quantity for Adventurer GPS Watch').fill('0');

  // Quantity changes flow through onChange -> updateBasket, so there is no
  // separate "Update" button; wait for the empty-cart state instead.
  await expect(page.getByText('Your shopping bag is empty')).toBeVisible();
});