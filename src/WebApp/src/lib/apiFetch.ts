// Shared by every src/api/*.ts file - collapses the fetch/status-check/error-throw
// boilerplate each one would otherwise repeat independently. treatAsNull lets a
// caller mark specific status codes (e.g. 404) as a real, non-error outcome rather
// than a thrown error.
//
// Every call sends X-CSRF: 1 -- Duende.BFF's BffAntiForgeryMiddleware requires this
// header on every request to a route mapped with AsBffApiEndpoint(), confirmed via
// a live smoke test against WebBFF (a request without it gets rejected with a 401
// before the handler even runs). The header's value doesn't matter to Duende, only
// its presence -- "1" is Duende's own documented convention.

interface RequestOptions {
  treatAsNull?: number[]
}

interface BodyOptions extends RequestOptions {
  body?: unknown
}

const csrfHeaders = { 'X-CSRF': '1' }

async function toResult<T>(response: Response, method: string, path: string, treatAsNull: number[] | undefined): Promise<T | null> {
  if (treatAsNull?.includes(response.status)) {
    return null
  }
  if (!response.ok) {
    const barePath = path.split('?')[0]
    throw new Error(`${method} ${barePath} failed: ${response.status}`)
  }
  if (response.status === 204) {
    return null
  }
  return (await response.json()) as T
}

function withJsonBody(body: unknown) {
  return { headers: { ...csrfHeaders, 'Content-Type': 'application/json' }, body: JSON.stringify(body) }
}

export function apiGet<T>(path: string): Promise<T>
export function apiGet<T>(path: string, options: { treatAsNull: number[] }): Promise<T | null>
export async function apiGet<T>(path: string, options?: RequestOptions): Promise<T | null> {
  const response = await fetch(path, { headers: csrfHeaders })
  return toResult<T>(response, 'GET', path, options?.treatAsNull)
}

export function apiPost<T>(path: string, options?: { body?: unknown }): Promise<T>
export function apiPost<T>(path: string, options: { body?: unknown; treatAsNull: number[] }): Promise<T | null>
export async function apiPost<T>(path: string, options?: BodyOptions): Promise<T | null> {
  const response = await fetch(path, {
    method: 'POST',
    ...(options?.body !== undefined ? withJsonBody(options.body) : { headers: csrfHeaders }),
  })
  return toResult<T>(response, 'POST', path, options?.treatAsNull)
}

export function apiPut<T>(path: string, options: { body: unknown }): Promise<T>
export function apiPut<T>(path: string, options: { body: unknown; treatAsNull: number[] }): Promise<T | null>
export async function apiPut<T>(path: string, options: BodyOptions): Promise<T | null> {
  const response = await fetch(path, { method: 'PUT', ...withJsonBody(options.body) })
  return toResult<T>(response, 'PUT', path, options.treatAsNull)
}

export function apiDelete<T>(path: string): Promise<T>
export function apiDelete<T>(path: string, options: { treatAsNull: number[] }): Promise<T | null>
export async function apiDelete<T>(path: string, options?: RequestOptions): Promise<T | null> {
  const response = await fetch(path, { method: 'DELETE', headers: csrfHeaders })
  return toResult<T>(response, 'DELETE', path, options?.treatAsNull)
}
