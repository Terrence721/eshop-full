import type { IncomingMessage } from 'node:http'
import react from '@vitejs/plugin-react'
import { defineConfig, type ProxyOptions } from 'vite'

// Identity.API's Quickstart controllers + Duende IdentityServer's own
// endpoints (/connect/*, /.well-known/*) proxy through so the browser sees
// one origin -- the existing ASP.NET Identity cookie just works, no CORS
// config needed on Identity.API (it isn't configured to allow any, and
// stays untouched). Real deployment topology (reverse proxy vs. something
// else under eShop.AppHost) is a separate, still-open decision -- this is
// dev-only.
const identityApiTarget = 'http://localhost:5223'

// Duende itself performs real top-level browser redirects to these same
// paths (e.g. /connect/authorize redirecting an unauthenticated browser to
// /Account/Login?returnUrl=...) -- a genuine navigation, not a fetch() call
// from inside the already-loaded SPA. Without this, that redirect (or a
// developer typing the URL directly) would land on Identity.API's raw JSON
// response instead of the React page built for it. A real browser
// navigation always sends an Accept header including text/html; this
// project's fetch() calls never set that (default Accept: */*), so it
// reliably tells the two cases apart without touching Identity.API at all.
function bypassTopLevelNavigation(req: IncomingMessage) {
  if (req.headers.accept?.includes('text/html')) {
    return '/index.html'
  }
}

// `changeOrigin: false` (the default) is documented to leave the Host header
// alone, but that doesn't hold up empirically here: a temporary diagnostic
// middleware on Identity.API confirmed it still receives Host:
// localhost:5223 (its own address) even when the browser's real request came
// in through this proxy at localhost:5177. Duende builds its own absolute
// redirect URLs (e.g. /connect/authorize -> /Account/Login) from
// Request.Host, so a real top-level browser navigation through this proxy
// got redirected off the SPA's origin entirely. Forcing the outgoing
// request's Host header back to the original one fixes it.
function preserveOriginalHost(proxy: Parameters<NonNullable<ProxyOptions['configure']>>[0]) {
  proxy.on('proxyReq', (proxyReq, req) => {
    if (req.headers.host) {
      proxyReq.setHeader('host', req.headers.host)
    }
  })
}

const quickstartAreaProxy: ProxyOptions = {
  target: identityApiTarget,
  bypass: bypassTopLevelNavigation,
  configure: preserveOriginalHost,
}

const identityServerProxy: ProxyOptions = {
  target: identityApiTarget,
  configure: preserveOriginalHost,
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/Home': quickstartAreaProxy,
      '/Account': quickstartAreaProxy,
      '/External': quickstartAreaProxy,
      '/Consent': quickstartAreaProxy,
      '/Device': quickstartAreaProxy,
      '/Diagnostics': quickstartAreaProxy,
      '/Grants': quickstartAreaProxy,
      '/connect': identityServerProxy,
      '/.well-known': identityServerProxy,
    },
  },
})
