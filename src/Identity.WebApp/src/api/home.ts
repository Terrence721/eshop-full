import { apiGet } from '../lib/apiFetch'

export interface HomeIndex {
  version: string
  wellKnownConfigurationUrl: string
  diagnosticsUrl: string
  grantsUrl: string
}

export interface HomeError {
  // Duende's ErrorMessage shape isn't verified against the real assembly --
  // reflection hit dependency-resolution friction. Null when errorId doesn't
  // match a real error context (confirmed via a real request); treated as an
  // opaque object otherwise until a real error page needs specific fields.
  error: Record<string, unknown> | null
}

// Real behavior, not a guess: this action returns 404 outside Development
// (Duende recommends disabling it in production) -- that's not an error.
export function getHomeIndex(): Promise<HomeIndex | null> {
  return apiGet<HomeIndex>('/Home/Index', { treatAsNull: [404] })
}

export function getHomeError(errorId: string): Promise<HomeError> {
  return apiGet<HomeError>(`/Home/Error?errorId=${encodeURIComponent(errorId)}`)
}
