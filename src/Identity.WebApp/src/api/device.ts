import type { ScopeViewModel } from './consent'

export interface DeviceAuthorizationViewModel {
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
  userCode: string
  confirmUserCode: boolean
}

export interface DeviceIndexResult {
  needsUserCode: boolean
  viewModel: DeviceAuthorizationViewModel | null
}

export interface DeviceCallbackResult {
  validationError: string | null
  viewModel: DeviceAuthorizationViewModel | null
}

export interface DeviceCallbackRequest {
  userCode: string
  button: 'yes' | 'no'
  scopesConsented: string[]
  rememberConsent: boolean
  description: string | null
}

// Real behavior: a missing/expired userCode returns NotFound() for both
// GET /Device/Index (when a code IS supplied) and POST
// /Device/UserCodeCapture -- there's no third "invalid code" state to
// represent, matching DeviceIndexResult's own doc comment.
export async function getDeviceIndex(userCode?: string): Promise<DeviceIndexResult | null> {
  const query = userCode ? `?userCode=${encodeURIComponent(userCode)}` : ''
  const response = await fetch(`/Device/Index${query}`)
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new Error(`GET /Device/Index failed: ${response.status}`)
  }
  return response.json()
}

// userCode binds from the query string, not JSON -- same [ApiController]
// simple-type inference already relied on for Logout/Grants.Revoke.
export async function captureUserCode(userCode: string): Promise<DeviceAuthorizationViewModel | null> {
  const response = await fetch(`/Device/UserCodeCapture?userCode=${encodeURIComponent(userCode)}`, {
    method: 'POST',
  })
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new Error(`POST /Device/UserCodeCapture failed: ${response.status}`)
  }
  return response.json()
}

// Real behavior, not a guess: DeviceController.Callback has three genuinely
// different outcomes -- 204 with no body when consent actually succeeded,
// 200 with a DeviceCallbackResult when the form needs redisplaying (a
// validation error), and 404 when the device-flow authorization vanished
// between page load and submit. A discriminated union instead of an
// overloaded null return, since "succeeded" and "not found" are both real,
// distinct outcomes the page needs to tell apart.
export type DeviceCallbackOutcome =
  | { outcome: 'success' }
  | { outcome: 'notFound' }
  | { outcome: 'redisplay'; result: DeviceCallbackResult }

export async function postDeviceCallback(request: DeviceCallbackRequest): Promise<DeviceCallbackOutcome> {
  const response = await fetch('/Device/Callback', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  if (response.status === 204) {
    return { outcome: 'success' }
  }
  if (response.status === 404) {
    return { outcome: 'notFound' }
  }
  if (!response.ok) {
    throw new Error(`POST /Device/Callback failed: ${response.status}`)
  }
  return { outcome: 'redisplay', result: await response.json() }
}
