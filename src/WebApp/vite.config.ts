/// <reference types="vitest/config" />
import react from '@vitejs/plugin-react'
import { defineConfig, type ProxyOptions } from 'vite'

// WebBFF holds the real OAuth tokens server-side and proxies every real
// backend call -- the browser never talks to Catalog.API/Basket.API/
// Ordering.API/Identity.API directly. Under eShop.AppHost,
// WithReference(webBff) injects the real dashboard-assigned port as
// services__webbff__http__0 (confirmed via a live probe, not assumed).
// Falls back to the old fixed port for running this dev server standalone,
// without AppHost.
const webBffTarget = process.env['services__webbff__http__0'] ?? 'http://localhost:5225'

// The OIDC handler builds its own absolute redirect_uri (for the
// /signin-oidc callback) from Request.Host when /bff/login's Challenge
// runs -- a real top-level browser navigation through this proxy needs
// that to stay this dev server's own host, or the identity provider
// redirects the browser back to WebBFF's raw address instead of here.
// `changeOrigin: false` (the default) is documented to leave the Host
// header alone, but Identity.WebApp/WebhookClient.WebApp's own proxies
// found that doesn't hold up empirically for this proxy library -- same
// underlying mechanism, so forcing it back explicitly here too.
function preserveOriginalHost(proxy: Parameters<NonNullable<ProxyOptions['configure']>>[0]) {
  proxy.on('proxyReq', (proxyReq, req) => {
    if (req.headers.host) {
      proxyReq.setHeader('host', req.headers.host)
    }
  })
}

const webBffProxy: ProxyOptions = {
  target: webBffTarget,
  configure: preserveOriginalHost,
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5045,
    proxy: {
      '/bff': webBffProxy,
      '/remoteapi': webBffProxy,
      '/api': webBffProxy,
      '/signin-oidc': webBffProxy,
      '/signout-callback-oidc': webBffProxy,
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
