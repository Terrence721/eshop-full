import { apiGet, apiPost } from '../lib/apiFetch'
import type { ScopeConsentRequest, ScopeConsentViewModel } from './consent'

export interface DeviceAuthorizationViewModel extends ScopeConsentViewModel {
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

export interface DeviceCallbackRequest extends ScopeConsentRequest {
  userCode: string
}

// Real behavior: a missing/expired userCode returns NotFound() for both
// GET /Device/Index (when a code IS supplied) and POST
// /Device/UserCodeCapture -- there's no third "invalid code" state to
// represent, matching DeviceIndexResult's own doc comment.
export function getDeviceIndex(userCode?: string): Promise<DeviceIndexResult | null> {
  const query = userCode ? `?userCode=${encodeURIComponent(userCode)}` : ''
  return apiGet<DeviceIndexResult>(`/Device/Index${query}`, { treatAsNull: [404] })
}

// userCode binds from the query string, not JSON -- same [ApiController]
// simple-type inference already relied on for Logout/Grants.Revoke.
export function captureUserCode(userCode: string): Promise<DeviceAuthorizationViewModel | null> {
  return apiPost<DeviceAuthorizationViewModel>(`/Device/UserCodeCapture?userCode=${encodeURIComponent(userCode)}`, {
    treatAsNull: [404],
  })
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
