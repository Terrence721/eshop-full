import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';
import path from 'path';

dotenv.config({ path: './.env' });

export default defineConfig({
  testDir: './e2e',
  testMatch: ['**/ClientAppBrowser.spec.ts'],
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:8080',
    trace: 'on-first-retry',
    ...devices['Desktop Chrome'],
  },
  webServer: {
    command: 'yarn run:clientapp:browser:publish && python -m http.server 8080 --directory artifacts/publish/ClientApp.Browser/release/wwwroot',
    url: 'http://localhost:8080',
    reuseExistingServer: true,
    stderr: 'pipe',
    stdout: 'pipe',
    timeout: 5 * 60_000,
  },
});