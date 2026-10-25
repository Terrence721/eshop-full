import { useEffect, useState } from 'react'
import { useLocation } from 'react-router'
import { useCurrentUser } from '../../lib/useCurrentUser'

// Landing page now shows the homepage image only; the product catalog grid,
// filters, and pagination are no longer rendered here. The person icon baked
// into the upper-right of eshop_homepage.png opens a login modal. After the
// OIDC round-trip completes and the redirect lands back on this page, the
// modal reopens showing "Login successful".
export default function CatalogIndex() {
  const location = useLocation()
  const { claims, loading } = useCurrentUser()
  const [open, setOpen] = useState(false)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    if (
      typeof window !== 'undefined' &&
      window.sessionStorage.getItem('login:pending') === '1' &&
      !loading &&
      claims
    ) {
      window.sessionStorage.removeItem('login:pending')
      setSuccess(true)
      setOpen(true)
    }
  }, [claims, loading])

  const startLogin = () => {
    window.sessionStorage.setItem('login:pending', '1')
    const returnUrl = location.pathname + location.search
    window.location.assign(`/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`)
  }

  return (
    <div style={{ position: 'relative', display: 'inline-block', width: '100%' }}>
      <img
        src="/eshop_homepage.png"
        alt="eShop homepage"
        className="homepage-image"
        style={{ width: '100%', height: 'auto', display: 'block' }}
      />
      <button
        type="button"
        aria-label="Log in"
        onClick={() => setOpen(true)}
        style={{
          position: 'absolute',
          top: '3%',
          right: '8.5%',
          width: '5%',
          height: '4%',
          background: 'transparent',
          border: 'none',
          cursor: 'pointer',
          padding: 0,
        }}
      />

      {open ? (
        <div
          role="dialog"
          aria-modal="true"
          style={{
            position: 'fixed',
            inset: 0,
            background: 'rgba(0,0,0,0.5)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1000,
          }}
          onClick={() => setOpen(false)}
        >
          <div
            style={{
              background: '#fff',
              color: '#000',
              padding: '24px',
              borderRadius: '8px',
              minWidth: '280px',
              textAlign: 'center',
            }}
            onClick={(e) => e.stopPropagation()}
          >
            {success ? (
              <>
                <p style={{ fontWeight: 600, marginBottom: '16px' }}>Login successful</p>
                <button type="button" onClick={() => { setOpen(false); setSuccess(false) }}>
                  Close
                </button>
              </>
            ) : claims ? (
              <>
                <p style={{ marginBottom: '16px' }}>You are already signed in.</p>
                <button type="button" onClick={() => setOpen(false)}>Close</button>
              </>
            ) : (
              <>
                <p style={{ marginBottom: '16px' }}>Sign in to your account</p>
                <button type="button" onClick={startLogin} style={{ marginRight: '8px' }}>
                  Log in
                </button>
                <button type="button" onClick={() => setOpen(false)}>Cancel</button>
              </>
            )}
          </div>
        </div>
      ) : null}
    </div>
  )
}
