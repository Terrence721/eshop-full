import { Link, useLocation } from 'react-router'
import { useCurrentUser } from '../lib/useCurrentUser'

// Mirrors Layout/UserMenu.razor. Unlike WebhookClient.WebApp's version,
// "Log in" goes through /user/login rather than straight to /bff/login --
// that page is the one place that decides whether to kick off the real
// OIDC challenge or bounce back to returnUrl, mirroring upstream's own
// LogIn.razor pass-through. "Log out" is a plain link to /user/logout,
// not a form POST -- that page reads /bff/user's management claim for
// Duende.BFF's real GET-based front-channel logout URL and navigates
// there itself.
export function UserMenu() {
  const { claims, loading } = useCurrentUser()
  const location = useLocation()

  if (loading) {
    return null
  }

  if (!claims) {
    const returnUrl = location.pathname + location.search
    return (
      <Link
        className="action avatar-link"
        to={`/user/login?returnUrl=${encodeURIComponent(returnUrl)}`}
        aria-label="Log in"
      >
        <svg
          width="28"
          height="28"
          viewBox="0 0 24 24"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          aria-hidden="true"
        >
          <circle cx="12" cy="12" r="11" stroke="currentColor" strokeWidth="2" />
          <circle cx="12" cy="9" r="3" fill="currentColor" />
          <path d="M6 19c0-3 3-5 6-5s6 2 6 5" fill="currentColor" />
        </svg>
      </Link>
    )
  }

  const name = claims.find((claim) => claim.type === 'name')?.value

  return (
    <>
      <strong>{name}</strong>
      <Link className="action" to="/user/logout">
        Log out
      </Link>
    </>
  )
}
