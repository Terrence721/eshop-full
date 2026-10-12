/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, type ProxyOptions } from 'vite'

// WebhookClient's own BFF/API endpoints proxy through so the browser sees
// one origin -- the httpOnly session cookie WebhookClient sets just works,
// no CORS config needed there (it isn't configured to allow any, and
// stays untouched). Real deployment topology is a separate, still-open
// decision -- this is dev-only.
const webhookClientTarget = 'http://localhost:5062'

// The OIDC handler builds its own absolute redirect_uri (for the
// /signin-oidc callback) from Request.Host when /bff/login's Challenge
// runs -- a real top-level browser navigation through this proxy needs
// that to stay this dev server's own host, or the identity provider
// redirects the browser back to WebhookClient's raw address instead of
// here. `changeOrigin: false` (the default) is documented to leave the
// Host header alone, but Identity.WebApp's own proxy found that doesn't
// hold up empirically for this proxy library -- same underlying
// mechanism, so forcing it back explicitly here too.
function preserveOriginalHost(proxy: Parameters<NonNullable<ProxyOptions['configure']>>[0]) {
  proxy.on('proxyReq', (proxyReq, req) => {
    if (req.headers.host) {
      proxyReq.setHeader('host', req.headers.host)
    }
  })
}

const webhookClientProxy: ProxyOptions = {
  target: webhookClientTarget,
  configure: preserveOriginalHost,
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/bff': webhookClientProxy,
      '/signin-oidc': webhookClientProxy,
      '/signout-callback-oidc': webhookClientProxy,
      '/logout': webhookClientProxy,
      '/api': webhookClientProxy,
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
