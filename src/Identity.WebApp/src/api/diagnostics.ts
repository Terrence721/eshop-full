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
export async function getDiagnostics(): Promise<DiagnosticsViewModel | null> {
  const response = await fetch('/Diagnostics/Index')
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new Error(`GET /Diagnostics/Index failed: ${response.status}`)
  }
  return response.json()
}
