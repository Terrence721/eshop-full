import { useEffect } from 'react'
import { Outlet, useLocation } from 'react-router'
import { redirectToLogin } from '../lib/auth'
import { useCurrentUser } from '../lib/useCurrentUser'

// SPA-side equivalent of upstream's [Authorize] attribute on
// CartPage/Checkout/Orders. Redirecting is a real top-level navigation
// (via auth.ts), not client-side routing, so it has to happen as a side
// effect once we know for certain the visitor is signed out -- not
// during render, which React (and React's own StrictMode double-invoke)
// doesn't allow side effects in.
export function RequireAuth() {
  const { claims, loading } = useCurrentUser()
  const location = useLocation()

  useEffect(() => {
    if (!loading && !claims) {
      redirectToLogin(location.pathname + location.search)
    }
  }, [loading, claims, location.pathname, location.search])

  if (loading || !claims) {
    return null
  }

  return <Outlet />
}
