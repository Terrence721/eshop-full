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
//
// WebBFF's YARP route only strips the "/remoteapi/orders" prefix off the
// client path -- it does not also strip "api/orders", which is
// Ordering.API's own real route group (confirmed in
// src/Ordering.API/Apis/OrdersApi.cs). Same situation as catalog.ts.
export function getOrders(): Promise<OrderSummary[]> {
  return apiGet<OrderSummary[]>('/remoteapi/orders/api/orders')
}
