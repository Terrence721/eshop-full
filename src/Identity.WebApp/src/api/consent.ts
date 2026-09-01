export interface ScopeViewModel {
  value: string
  displayName: string
  description: string | null
  emphasize: boolean
  required: boolean
  checked: boolean
}

export interface ConsentViewModel {
  button: string | null
  scopesConsented: string[] | null
  rememberConsent: boolean
  returnUrl: string | null
  description: string | null
  clientName: string
  clientUrl: string | null
  clientLogoUrl: string | null
  allowRememberConsent: boolean
  identityScopes: ScopeViewModel[]
  apiScopes: ScopeViewModel[]
}

export interface ConsentPostResult {
  redirectUrl: string | null
  isNativeClient: boolean
  validationError: string | null
  viewModel: ConsentViewModel | null
}

export interface ConsentRequest {
  button: 'yes' | 'no'
  scopesConsented: string[]
  rememberConsent: boolean
  returnUrl: string
  description: string | null
}

// Real behavior for both actions below: a 404 means no authorization
// request matches this returnUrl (expired, already used, or a stale
// link) -- ConsentController.Index returns NotFound() in that case for
// both GET and POST, not a validation error. Treated as null, not thrown,
// matching getHomeIndex's precedent for a real non-error 404.

export async function getConsent(returnUrl: string): Promise<ConsentViewModel | null> {
  const response = await fetch(`/Consent/Index?returnUrl=${encodeURIComponent(returnUrl)}`)
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new Error(`GET /Consent/Index failed: ${response.status}`)
  }
  return response.json()
}

export async function postConsent(request: ConsentRequest): Promise<ConsentPostResult | null> {
  const response = await fetch('/Consent/Index', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new Error(`POST /Consent/Index failed: ${response.status}`)
  }
  return response.json()
}
