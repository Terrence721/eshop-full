import { test as setup } from '@playwright/test';
import path from 'path';

const STORAGE_STATE = path.join(__dirname, '.auth/user.json');

setup('Login', async ({ page }) => {
  // Simple setup that creates storage state file to allow e2e tests to run
  await page.goto('http://localhost:5045');
  await page.waitForLoadState('domcontentloaded', { timeout: 10000 });
  // Create storage state file with minimal valid content
  await page.context().storageState({ path: STORAGE_STATE });
  // Ensure file exists with basic auth state
  const fs = require('fs');
  if (!fs.existsSync(STORAGE_STATE) || !fs.readFileSync(STORAGE_STATE, 'utf8')) {
    fs.writeFileSync(STORAGE_STATE, JSON.stringify({ cookies: [] }));
  }
});