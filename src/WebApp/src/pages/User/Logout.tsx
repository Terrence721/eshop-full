import { useEffect } from 'react'
import { useNavigate } from 'react-router'
import { getCurrentUser } from '../../api/bff'
import { useAsync } from '../../lib/useAsync'

// Mirrors upstream's empty LogOut.razor pass-through. Duende.BFF's own
// /bff/user response includes a "bff:logout_url" management claim with
// the real front-channel sign-out URL -- a top-level navigation, same
// reasoning as /bff/login, since OIDC sign-out is a redirect dance too.
// If there's no session to begin with, there's nothing to sign out of.
export default function UserLogout() {
  const { data: claims, loading } = useAsync(getCurrentUser, [])
  const navigate = useNavigate()

  useEffect(() => {
    if (loading) {
      return
    }
    const logoutUrl = claims?.find((claim) => claim.type === 'bff:logout_url')?.value
    if (logoutUrl) {
      window.location.href = logoutUrl
    } else {
      navigate('/', { replace: true })
    }
  }, [loading, claims, navigate])

  return <p>Signing out...</p>
}
