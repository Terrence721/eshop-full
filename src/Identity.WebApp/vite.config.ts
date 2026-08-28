import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Identity.API's Quickstart controllers + Duende IdentityServer's own
// endpoints (/connect/*, /.well-known/*) proxy through so the browser sees
// one origin -- the existing ASP.NET Identity cookie just works, no CORS
// config needed on Identity.API (it isn't configured to allow any, and
// stays untouched). Real deployment topology (reverse proxy vs. something
// else under eShop.AppHost) is a separate, still-open decision -- this is
// dev-only.
const identityApiTarget = 'http://localhost:5223'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/Home': identityApiTarget,
      '/Account': identityApiTarget,
      '/External': identityApiTarget,
      '/Consent': identityApiTarget,
      '/Device': identityApiTarget,
      '/Diagnostics': identityApiTarget,
      '/Grants': identityApiTarget,
      '/connect': identityApiTarget,
      '/.well-known': identityApiTarget,
    },
  },
})
