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

export async function getHomeIndex(): Promise<HomeIndex | null> {
  const response = await fetch('/Home/Index')
  // Real behavior, not a guess: this action returns 404 outside Development
  // (Duende recommends disabling it in production) -- that's not an error.
  if (response.status === 404) {
    return null
  }
  if (!response.ok) {
    throw new Error(`GET /Home/Index failed: ${response.status}`)
  }
  return response.json()
}

export async function getHomeError(errorId: string): Promise<HomeError> {
  const response = await fetch(`/Home/Error?errorId=${encodeURIComponent(errorId)}`)
  if (!response.ok) {
    throw new Error(`GET /Home/Error failed: ${response.status}`)
  }
  return response.json()
}
