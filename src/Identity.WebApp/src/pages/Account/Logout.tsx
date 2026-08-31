import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import { buildLogoutFormAction, getLogout, postLogout } from '../../api/account'

function LogoutPage() {
  const [searchParams] = useSearchParams()
  const logoutId = searchParams.get('logoutId')
  const navigate = useNavigate()

  const [showPrompt, setShowPrompt] = useState(false)
  const [signingOut, setSigningOut] = useState(false)
  const [loadError, setLoadError] = useState<Error | null>(null)

  const confirmLogout = useCallback(async () => {
    setSigningOut(true)
    try {
      const result = await postLogout(logoutId)
      navigate(`/Account/LoggedOut?logoutId=${encodeURIComponent(result.logoutId ?? '')}`)
    } catch {
      // The external-IdP case: postLogout's redirect went cross-origin,
      // which fetch() can't follow. A real <form> POST lets the browser
      // handle the whole round trip natively.
      const form = document.createElement('form')
      form.method = 'POST'
      form.action = buildLogoutFormAction(logoutId)
      document.body.appendChild(form)
      form.submit()
    }
  }, [logoutId, navigate])

  useEffect(() => {
    let cancelled = false
    getLogout(logoutId)
      .then(async (vm) => {
        if (cancelled) {
          return
        }
        if (vm.showLogoutPrompt) {
          setShowPrompt(true)
        } else {
          // Safe to sign out immediately -- Duende already determined no
          // confirmation is needed (e.g. the interaction context says so).
          await confirmLogout()
        }
      })
      .catch((error) => {
        if (!cancelled) {
          setLoadError(error)
        }
      })
    return () => {
      cancelled = true
    }
  }, [logoutId, confirmLogout])

  if (loadError) {
    return <p>Could not load the logout page: {loadError.message}</p>
  }

  if (signingOut || !showPrompt) {
    return <p>Signing out...</p>
  }

  return (
    <div>
      <h1>Logout</h1>
      <p>Are you sure you want to log out?</p>
      <button onClick={confirmLogout}>Yes, log me out</button>
    </div>
  )
}

export default LogoutPage
