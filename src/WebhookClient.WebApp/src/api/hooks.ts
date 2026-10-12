import { apiGet } from '../lib/apiFetch'

export interface WebhookResponse {
  date: string
  destUrl: string | null
  token: string | null
}

export interface WebhookSubscriptionRequest {
  url: string
  token: string
  event: string
  grantUrl: string
}

export function getRegisteredHooks(): Promise<WebhookResponse[]> {
  return apiGet<WebhookResponse[]>('/api/hooks')
}

// Mirrors AddWebhook.razor's options.Value.Token pre-fill -- the shared
// secret Webhooks.API validates a registration's token against, not just a
// default value.
export function getDefaultToken(): Promise<{ token: string | null }> {
  return apiGet<{ token: string | null }>('/api/hooks/default-token')
}

// HooksApiEndpoints.MapPost("/api/hooks") forwards WebhooksClient's raw
// HttpStatusCode with no response body -- apiPost expects a JSON body on
// success, so this stays on a plain fetch() instead, same carve-out
// apiFetch.ts documents for responses that don't fit its shape.
export async function registerHook(request: WebhookSubscriptionRequest): Promise<{ ok: boolean; status: number }> {
  const response = await fetch('/api/hooks', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  return { ok: response.ok, status: response.status }
}
