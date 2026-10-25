import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router'

const DEMO_USER_KEY = 'demo:user'

// Demo-only sign-in: there is no server-side session here, just a client-side
// flag so the UI can mimic a successful login. Not real authentication.
export default function CatalogIndex() {
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [currentUser, setCurrentUser] = useState<string | null>(() =>
    typeof window === 'undefined'
      ? null
      : window.sessionStorage.getItem(DEMO_USER_KEY),
  )
  const [success, setSuccess] = useState<boolean>(
    typeof window !== 'undefined' &&
      !!window.sessionStorage.getItem(DEMO_USER_KEY),
  )

  useEffect(() => {
    if (typeof window !== 'undefined') {
      const saved = window.sessionStorage.getItem(DEMO_USER_KEY)
      if (saved) {
        setCurrentUser(saved)
        setSuccess(true)
      }
    }
  }, [])

  const signIn = (event: React.FormEvent) => {
    event.preventDefault()
    if (!username.trim()) {
      setError('Username is required')
      return
    }
    if (!password) {
      setError('Password is required')
      return
    }
    window.sessionStorage.setItem(DEMO_USER_KEY, username.trim())
    setCurrentUser(username.trim())
    setPassword('')
    setError('')
    setSuccess(true)
  }

  const signOut = () => {
    window.sessionStorage.removeItem(DEMO_USER_KEY)
    setCurrentUser(null)
    setSuccess(false)
    setUsername('')
    setPassword('')
    setError('')
    setOpen(false)
  }

  return (
    <div style={{ position: 'relative', display: 'inline-block', width: '100%', aspectRatio: '1757 / 1660' }}>
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
          zIndex: 2,
          padding: 0,
        }}
      />
      <button type="button" aria-label="All catalog items" onClick={() => navigate('/catalog')} style={{ position: 'absolute', top: '50%', left: '4.5%', width: '3%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />
      <button type="button" aria-label="AirStrider catalog items" onClick={() => navigate('/catalog?brand=9')} style={{ position: 'absolute', top: '49.5%', left: '8.5%', width: '4%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />
      <button type="button" aria-label="B and R catalog items" onClick={() => navigate('/catalog?brand=5')} style={{ position: 'absolute', top: '49.5%', left: '13.5%', width: '3.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />
      <button type="button" aria-label="Daybird catalog items" onClick={() => navigate('/catalog?brand=1')} style={{ position: 'absolute', top: '52.5%', left: '5.5%', width: '4%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />
      <button type="button" aria-label="Gravitator catalog items" onClick={() => navigate('/catalog?brand=2')} style={{ position: 'absolute', top: '52.5%', left: '10.5%', width: '4.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />
      <button type="button" aria-label="Green Equipment catalog items" onClick={() => navigate('/catalog?brand=10')} style={{ position: 'absolute', top: '55.5%', left: '8.5%', width: '5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />
      <button type="button" aria-label="Legend catalog items" onClick={() => navigate('/catalog?brand=12')} style={{ position: 'absolute', top: '58.5%', left: '5.5%', width: '3.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />

      <button type="button" aria-label="Quester catalog items" onClick={() => navigate('/catalog?brand=4')} style={{ position: 'absolute', top: '58.5%', left: '9.5%', width: '3.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />

      <button type="button" aria-label="Raptor Elite catalog items" onClick={() => navigate('/catalog?brand=6')} style={{ position: 'absolute', top: '61.5%', left: '6.5%', width: '4.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />

      <button type="button" aria-label="Solstix catalog items" onClick={() => navigate('/catalog?brand=7')} style={{ position: 'absolute', top: '61.5%', left: '13%', width: '4%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />

      <button type="button" aria-label="WildRunner catalog items" onClick={() => navigate('/catalog?brand=3')} style={{ position: 'absolute', top: '64.5%', left: '7.5%', width: '4.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />

      <button type="button" aria-label="XE catalog items" onClick={() => navigate('/catalog?brand=13')} style={{ position: 'absolute', top: '64.5%', left: '11.5%', width: '2.5%', height: '1.5%', background: 'transparent', border: 'none', cursor: 'pointer', zIndex: 2, padding: 0 }} />

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
            {currentUser && success ? (
              <>
                <p style={{ fontWeight: 600, marginBottom: '16px' }}>
                  Login successful
                </p>
                <p style={{ marginBottom: '16px' }}>Welcome, {currentUser}!</p>
                <button type="button" onClick={() => setOpen(false)} style={{ marginRight: '8px' }}>
                  Close
                </button>
                <button type="button" onClick={signOut}>Sign out</button>
              </>
            ) : currentUser ? (
              <>
                <p style={{ marginBottom: '16px' }}>Signed in as {currentUser}</p>
                <button type="button" onClick={() => setOpen(false)} style={{ marginRight: '8px' }}>
                  Close
                </button>
                <button type="button" onClick={signOut}>Sign out</button>
              </>
            ) : (
              <form onSubmit={signIn}>
                <p style={{ fontWeight: 600, marginBottom: '16px' }}>Sign in</p>
                <div style={{ marginBottom: '12px' }}>
                  <input
                    type="text"
                    placeholder="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                  />
                </div>
                <div style={{ marginBottom: '12px' }}>
                  <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                  />
                </div>
                {error ? (
                  <p style={{ color: 'red', marginBottom: '12px' }}>{error}</p>
                ) : null}
                <button type="submit" style={{ marginRight: '8px' }}>Log in</button>
                <button type="button" onClick={() => setOpen(false)}>Cancel</button>
              </form>
            )}
          </div>
        </div>
      ) : null}
    </div>
  )
}