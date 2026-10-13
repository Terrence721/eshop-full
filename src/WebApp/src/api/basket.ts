import { apiDelete, apiGet, apiPut } from '../lib/apiFetch'

export interface BasketQuantity {
  productId: number
  quantity: number
}

// WebBFF owns a native-gRPC Basket client server-side (no browser gRPC-Web
// tooling exists in this fork) and exposes these as plain REST endpoints.
// PUT, not POST: Basket.API's gRPC UpdateBasket is always a full replace,
// matching how upstream's own BasketState.SetQuantityAsync/AddAsync
// always resend the whole basket rather than a single line delta.
export function getBasket(): Promise<BasketQuantity[]> {
  return apiGet<BasketQuantity[]>('/api/basket')
}

// The body is the bare array itself, not { items: [...] } -- WebBFF's PUT
// handler binds its IReadOnlyCollection<BasketQuantity> parameter directly
// from the request body, with no wrapper envelope.
export function updateBasket(items: BasketQuantity[]): Promise<BasketQuantity[]> {
  return apiPut<BasketQuantity[]>('/api/basket', { body: items })
}

export function clearBasket(): Promise<null> {
  return apiDelete<null>('/api/basket')
}
