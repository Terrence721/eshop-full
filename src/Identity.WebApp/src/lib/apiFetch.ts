// Shared by every src/api/*.ts file - collapses the fetch/status-check/error-throw
// boilerplate each one used to repeat independently. treatAsNull lets a caller
// mark specific status codes (e.g. 404) as a real, non-error outcome rather than
// a thrown error, matching this app's established "loopback-only 404 is not an
// error" precedent.
//
// Not every endpoint fits this shape - postDeviceCallback's three-way
// 204/404/redisplay branching genuinely needs its own control flow and stays on
// raw fetch() rather than being forced through here.

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
    // Matches every api/*.ts file's original error format, which never
    // included query parameters (e.g. "GET /Home/Error failed: 500", not
    // "GET /Home/Error?errorId=... failed: 500") -- keep messages stable
    // for callers/tests, and avoid echoing query values into error text.
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
