// Shared by every src/api/*.ts file - collapses the fetch/status-check/error-throw
// boilerplate each one used to repeat independently. treatAsNull lets a caller
// mark specific status codes (e.g. 401) as a real, non-error outcome rather than
// a thrown error, matching this app's established "not-logged-in is not an
// error" precedent.

interface RequestOptions {
  treatAsNull?: number[]
}

interface PostOptions extends RequestOptions {
  body?: unknown
}

async function toResult<T>(response: Response, method: string, path: string, treatAsNull: number[] | undefined): Promise<T | null> {
  if (treatAsNull?.includes(response.status)) {
    return null
  }
  if (!response.ok) {
    const barePath = path.split('?')[0]
    throw new Error(`${method} ${barePath} failed: ${response.status}`)
  }
  return (await response.json()) as T
}

export function apiGet<T>(path: string): Promise<T>
export function apiGet<T>(path: string, options: { treatAsNull: number[] }): Promise<T | null>
export async function apiGet<T>(path: string, options?: RequestOptions): Promise<T | null> {
  const response = await fetch(path)
  return toResult<T>(response, 'GET', path, options?.treatAsNull)
}

export function apiPost<T>(path: string, options?: { body?: unknown }): Promise<T>
export function apiPost<T>(path: string, options: { body?: unknown; treatAsNull: number[] }): Promise<T | null>
export async function apiPost<T>(path: string, options?: PostOptions): Promise<T | null> {
  const response = await fetch(path, {
    method: 'POST',
    ...(options?.body !== undefined
      ? { headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(options.body) }
      : {}),
  })
  return toResult<T>(response, 'POST', path, options?.treatAsNull)
}
