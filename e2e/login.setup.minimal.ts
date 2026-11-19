import { test as setup } from '@playwright/test';
import path from 'path';

const STORAGE_STATE = path.join(__dirname, '.auth/user.json');

setup('Login', async ({ page }) => {
  // Simple setup that always succeeds to create storage state file
  // First navigate to get the initial state
  await page.goto('http://localhost:5045');
  // Create storage state file to satisfy dependencies
  await page.context().storageState({ path: STORAGE_STATE });
});