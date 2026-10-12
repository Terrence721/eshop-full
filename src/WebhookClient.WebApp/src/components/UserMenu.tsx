import { useLocation } from 'react-router'
import { useCurrentUser } from '../lib/useCurrentUser'

// Mirrors Layout/UserMenu.razor. The logout button is a plain uncontrolled
// <form> (a real browser POST navigation, not a fetch) -- SignOutAsync's
// OIDC front-channel sign-out redirects the browser to the identity
// provider and back, which only works as a genuine top-level navigation,
// same reasoning as vite.config.ts's Host-preservation for /bff/login.
export function UserMenu() {
  const { claims, loading } = useCurrentUser()
  const location = useLocation()

  if (loading) {
    return null
  }

  if (!claims) {
    const returnUrl = location.pathname + location.search
    const logIn = () => {
      window.location.href = `/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`
    }
    return (
      <button className="action" onClick={logIn}>
        Log in
      </button>
    )
  }

  const name = claims.find((claim) => claim.type === 'name')?.value

  return (
    <>
      <strong>{name}</strong>
      <form method="post" action="/logout">
        <button className="action" type="submit">
          Log out
        </button>
      </form>
    </>
  )
}
