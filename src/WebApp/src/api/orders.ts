import { apiGet } from '../lib/apiFetch'

// Matches eShop.Ordering.API.Application.Queries.OrderViewModel's real
// OrderSummary record.
export interface OrderSummary {
  orderNumber: number
  date: string
  status: string
  total: number
}

// Scoped server-side to the authenticated user via their "sub" claim -- no
// buyerId param to send, matching the real Ordering.API contract.
export function getOrders(): Promise<OrderSummary[]> {
  return apiGet<OrderSummary[]>('/remoteapi/orders')
}
