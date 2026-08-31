export interface ExternalProvider {
  displayName: string | null
  authenticationScheme: string
}

export interface LoginViewModel {
  username: string | null
  returnUrl: string | null
  rememberLogin: boolean
  allowRememberLogin: boolean
  enableLocalLogin: boolean
  externalProviders: ExternalProvider[]
  visibleExternalProviders: ExternalProvider[]
  isExternalLoginOnly: boolean
  externalLoginScheme: string | null
}

export interface LoginPostResult {
  redirectUrl: string | null
  isNativeClient: boolean
  viewModel: LoginViewModel | null
  validationError: string | null
}

export interface LoginRequest {
  username: string
  password: string
  rememberLogin: boolean
  returnUrl: string | null
}

export interface LogoutViewModel {
  logoutId: string | null
  showLogoutPrompt: boolean
}

export async function getLogin(returnUrl: string | null): Promise<LoginViewModel> {
  const query = returnUrl ? `?returnUrl=${encodeURIComponent(returnUrl)}` : ''
  const response = await fetch(`/Account/Login${query}`)
  if (!response.ok) {
    throw new Error(`GET /Account/Login failed: ${response.status}`)
  }
  return response.json()
}

export async function postLogin(request: LoginRequest): Promise<LoginPostResult> {
  const response = await fetch('/Account/Login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  if (!response.ok) {
    throw new Error(`POST /Account/Login failed: ${response.status}`)
  }
  return response.json()
}

export async function postLoginCancel(returnUrl: string | null): Promise<LoginPostResult> {
  const query = returnUrl ? `?returnUrl=${encodeURIComponent(returnUrl)}` : ''
  const response = await fetch(`/Account/LoginCancel${query}`, { method: 'POST' })
  if (!response.ok) {
    throw new Error(`POST /Account/LoginCancel failed: ${response.status}`)
  }
  return response.json()
}

export async function getLogout(logoutId: string | null): Promise<LogoutViewModel> {
  const query = logoutId ? `?logoutId=${encodeURIComponent(logoutId)}` : ''
  const response = await fetch(`/Account/Logout${query}`)
  if (!response.ok) {
    throw new Error(`GET /Account/Logout failed: ${response.status}`)
  }
  return response.json()
}

export interface LoggedOutViewModel {
  postLogoutRedirectUri: string | null
  clientName: string | null
  signOutIframeUrl: string | null
  automaticRedirectAfterSignOut: boolean
  logoutId: string | null
  triggerExternalSignout: boolean
  externalAuthenticationScheme: string | null
}

export async function getLoggedOut(logoutId: string | null): Promise<LoggedOutViewModel> {
  const query = logoutId ? `?logoutId=${encodeURIComponent(logoutId)}` : ''
  const response = await fetch(`/Account/LoggedOut${query}`)
  if (!response.ok) {
    throw new Error(`GET /Account/LoggedOut failed: ${response.status}`)
  }
  return response.json()
}

// AccountController.LogoutPost (routed to /Account/Logout) now always
// redirects rather than ever returning LoggedOutViewModel directly (fixed
// 2026-08-31, see todo.md) -- the common case (no external identity
// provider on the session) redirects same-origin to /Account/LoggedOut,
// which fetch() follows transparently per the Fetch spec (verified for
// real against the live server: a POST redirected 302 comes back as a 200
// with redirected:true and the real LoggedOutViewModel body, method
// correctly rewritten POST->GET). The rare case (an external IdP attached)
// redirects cross-origin instead, which a browser's fetch() cannot follow
// under its default 'cors' mode without the external provider granting
// CORS -- foundational, long-standing Fetch spec behavior, not something
// this session re-verified in an actual browser. Callers must catch a
// thrown error here and fall back to a real <form> POST (full navigation)
// to let the browser handle that case natively.
//
// logoutId is a query parameter, not a JSON body -- deliberately, so that
// exact same fallback <form> (which can only send
// application/x-www-form-urlencoded, never JSON) hits the identical URL
// this function does and binds correctly. Verified for real: a
// form-urlencoded POST against the old JSON-bound version came back 415;
// against this query-string version it redirects correctly, same as fetch().
export async function postLogout(logoutId: string | null): Promise<LoggedOutViewModel> {
  const query = logoutId ? `?logoutId=${encodeURIComponent(logoutId)}` : ''
  const response = await fetch(`/Account/Logout${query}`, { method: 'POST' })
  if (!response.ok) {
    throw new Error(`POST /Account/Logout failed: ${response.status}`)
  }
  return response.json()
}
