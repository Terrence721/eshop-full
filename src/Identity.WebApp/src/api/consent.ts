import { apiGet, apiPost } from '../lib/apiFetch'

export interface ScopeViewModel {
  value: string
  displayName: string
  description: string | null
  emphasize: boolean
  required: boolean
  checked: boolean
}

export interface ConsentViewModel {
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

export function getConsent(returnUrl: string): Promise<ConsentViewModel | null> {
  return apiGet<ConsentViewModel>(`/Consent/Index?returnUrl=${encodeURIComponent(returnUrl)}`, { treatAsNull: [404] })
}

export function postConsent(request: ConsentRequest): Promise<ConsentPostResult | null> {
  return apiPost<ConsentPostResult>('/Consent/Index', { body: request, treatAsNull: [404] })
}
