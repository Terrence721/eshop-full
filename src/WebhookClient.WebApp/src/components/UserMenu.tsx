import { useCurrentUser } from '../lib/useCurrentUser'

function logIn() {
  const returnUrl = window.location.pathname + window.location.search
  window.location.href = `/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`
}

// Mirrors Layout/UserMenu.razor. The logout button is a plain uncontrolled
// <form> (a real browser POST navigation, not a fetch) -- SignOutAsync's
// OIDC front-channel sign-out redirects the browser to the identity
// provider and back, which only works as a genuine top-level navigation,
// same reasoning as vite.config.ts's Host-preservation for /bff/login.
export function UserMenu() {
  const { claims, loading } = useCurrentUser()

  if (loading) {
    return null
  }

  if (!claims) {
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
