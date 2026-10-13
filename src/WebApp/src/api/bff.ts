import { apiGet } from '../lib/apiFetch'

export interface UserClaim {
  type: string
  value: string
}

// BffEndpoints.MapGet("/bff/user") returns 401 when the session cookie is
// missing or expired -- that's a real, expected "not logged in" outcome,
// not an error.
export function getCurrentUser(): Promise<UserClaim[] | null> {
  return apiGet<UserClaim[]>('/bff/user', { treatAsNull: [401] })
}
