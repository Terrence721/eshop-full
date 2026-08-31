import { useEffect, useState, type SubmitEvent } from 'react'
import { useSearchParams } from 'react-router'
import { getLogin, postLogin, postLoginCancel, type LoginViewModel } from '../../api/account'
import { buildExternalChallengeUrl } from '../../api/external'

function LoginPage() {
  const [searchParams] = useSearchParams()
  const returnUrl = searchParams.get('returnUrl')

  const [vm, setVm] = useState<LoginViewModel | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<Error | null>(null)

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [rememberLogin, setRememberLogin] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [validationError, setValidationError] = useState<string | null>(null)

  useEffect(() => {
    getLogin(returnUrl)
      .then(setVm)
      .catch(setLoadError)
      .finally(() => setLoading(false))
  }, [returnUrl])

  // Duende short-circuits the UI to the one external IdP a client
  // restricts login to -- there's no local form to show at all here.
  useEffect(() => {
    if (vm?.isExternalLoginOnly && vm.externalLoginScheme) {
      window.location.href = buildExternalChallengeUrl(vm.externalLoginScheme, vm.returnUrl)
    }
  }, [vm])

  if (loading) {
    return <p>Loading...</p>
  }

  if (loadError) {
    return <p>Could not load the login page: {loadError.message}</p>
  }

  if (!vm) {
    return null
  }

  if (vm.isExternalLoginOnly) {
    return <p>Redirecting to sign-in...</p>
  }

  async function handleSubmit(event: SubmitEvent) {
    event.preventDefault()
    setSubmitting(true)
    setValidationError(null)
    try {
      const result = await postLogin({ username, password, rememberLogin, returnUrl })
      if (result.validationError) {
        setValidationError(result.validationError)
        if (result.viewModel) {
          setVm(result.viewModel)
        }
        return
      }
      if (result.redirectUrl) {
        // A real navigation, not client-side routing: this may be an OIDC
        // callback URL that needs to complete the protocol redirect chain
        // back to whatever client requested the login. IsNativeClient
        // handling (a "click to continue" page instead of auto-redirecting,
        // for embedded-webview clients like the future ClientApp MAUI app)
        // is deliberately deferred -- no native client exists in this repo
        // yet, and its real redirect URI scheme isn't known.
        window.location.href = result.redirectUrl
      }
    } catch (error) {
      setValidationError(error instanceof Error ? error.message : 'Login failed.')
    } finally {
      setSubmitting(false)
    }
  }

  async function handleCancel() {
    setSubmitting(true)
    try {
      const result = await postLoginCancel(returnUrl)
      if (result.redirectUrl) {
        window.location.href = result.redirectUrl
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div>
      <h1>Login</h1>
      {validationError && <p role="alert">{validationError}</p>}

      {vm.enableLocalLogin && (
        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="username">Username</label>
            <input
              id="username"
              value={username}
              onChange={(event) => setUsername(event.target.value)}
              required
            />
          </div>
          <div>
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              required
            />
          </div>
          {vm.allowRememberLogin && (
            <div>
              <label htmlFor="rememberLogin">
                <input
                  id="rememberLogin"
                  type="checkbox"
                  checked={rememberLogin}
                  onChange={(event) => setRememberLogin(event.target.checked)}
                />
                Remember me
              </label>
            </div>
          )}
          <button type="submit" disabled={submitting}>
            Login
          </button>
          <button type="button" onClick={handleCancel} disabled={submitting}>
            Cancel
          </button>
        </form>
      )}

      {vm.visibleExternalProviders.length > 0 && (
        <div>
          <h2>External login</h2>
          <ul>
            {vm.visibleExternalProviders.map((provider) => (
              <li key={provider.authenticationScheme}>
                <a href={buildExternalChallengeUrl(provider.authenticationScheme, vm.returnUrl)}>
                  {provider.displayName}
                </a>
              </li>
            ))}
          </ul>
        </div>
      )}

      {!vm.enableLocalLogin && vm.visibleExternalProviders.length === 0 && (
        <p>No login method is available.</p>
      )}
    </div>
  )
}

export default LoginPage
