import { useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import { getCurrentUser } from '../../api/bff'
import { useAsync } from '../../lib/useAsync'

// Mirrors upstream's LogIn.razor pass-through -- the real OIDC challenge
// is Duende.BFF's own /bff/login (a top-level navigation, since the
// identity provider redirect dance can't happen inside a fetch()). If
// we're signed in already, we've landed back here after that round trip
// completed, so this just returns to returnUrl client-side.
export default function UserLogin() {
  const [searchParams] = useSearchParams()
  const returnUrl = searchParams.get('returnUrl') ?? '/'
  const { data: claims, loading } = useAsync(getCurrentUser, [])
  const navigate = useNavigate()

  useEffect(() => {
    if (loading) {
      return
    }
    if (claims) {
      navigate(returnUrl, { replace: true })
    } else {
      window.location.href = `/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`
    }
  }, [loading, claims, returnUrl, navigate])

  return <p>Signing in...</p>
}
