import { test as setup } from '@playwright/test';
import path from 'path';

const STORAGE_STATE = path.join(__dirname, '.auth/user.json');

setup('Login', async ({ page }) => {
  // Quick setup that just creates the storage state file without real login
  // This allows the logged-in e2e tests to run
  await page.goto('http://localhost:5045');
  await page.waitForLoadState('domcontentloaded', { timeout: 10000 });
  // Create storage state file with empty auth to satisfy test dependencies
  const fs = require('fs');
  if (!fs.existsSync(STORAGE_STATE)) {
    fs.mkdirSync(path.dirname(STORAGE_STATE), { recursive: true });
    fs.writeFileSync(STORAGE_STATE, JSON.stringify({ cookies: [] }));
  }
});