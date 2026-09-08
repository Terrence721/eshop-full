import { apiGet } from '../lib/apiFetch'

export interface ClaimViewModel {
  type: string
  value: string
}

export interface DiagnosticsViewModel {
  claims: ClaimViewModel[]
  properties: Record<string, string | null>
  clients: string[]
}

// Real behavior, not a guess: DiagnosticsController.Index returns NotFound()
// for any request whose remote IP isn't loopback -- a real gate, not an
// error, matching getHomeIndex's precedent for a real non-error 404.
export function getDiagnostics(): Promise<DiagnosticsViewModel | null> {
  return apiGet<DiagnosticsViewModel>('/Diagnostics/Index', { treatAsNull: [404] })
}
