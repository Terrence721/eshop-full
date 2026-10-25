import { Link } from 'react-router'
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

  if (loading) {
    return null
  }

  if (!claims) {
    // No login button on the landing UI; signed-out users just see no account affordance.
    return null
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
