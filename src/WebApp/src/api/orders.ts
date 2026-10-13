import { apiGet } from '../lib/apiFetch'

// Matches eShop.Ordering.API.Application.Queries.OrderViewModel's real
// OrderSummary/CardType records.
export interface OrderSummary {
  orderNumber: number
  date: string
  status: string
  total: number
}

export interface CardType {
  id: number
  name: string
}

// Matches eShop.Ordering.API.Application.Models.BasketItem (Ordering.API's
// own domain type, distinct from Basket.API's gRPC message and from
// useBasket's BasketQuantity) and OrdersApi.cs's own CreateOrderRequest
// record.
export interface OrderBasketItem {
  productId: number
  productName: string
  unitPrice: number
  oldUnitPrice: number
  quantity: number
  pictureUrl: string
}

export interface CreateOrderRequest {
  userId: string
  userName: string
  city: string
  street: string
  state: string
  country: string
  zipCode: string
  cardNumber: string
  cardHolderName: string
  cardExpiration: string
  cardSecurityNumber: string
  cardTypeId: number
  buyer: string
  items: OrderBasketItem[]
}

const ordersBase = '/remoteapi/orders/api/orders'

// Scoped server-side to the authenticated user via their "sub" claim -- no
// buyerId param to send, matching the real Ordering.API contract.
//
// WebBFF's YARP route only strips the "/remoteapi/orders" prefix off the
// client path -- it does not also strip "api/orders", which is
// Ordering.API's own real route group (confirmed in
// src/Ordering.API/Apis/OrdersApi.cs). Same situation as catalog.ts.
export function getOrders(): Promise<OrderSummary[]> {
  return apiGet<OrderSummary[]>(ordersBase)
}

export function getCardTypes(): Promise<CardType[]> {
  return apiGet<CardType[]>(`${ordersBase}/cardtypes`)
}

// CreateOrderAsync's real response is a bare 200 OK with no body on
// success (Results<Ok, BadRequest<string>>) -- apiPost expects a JSON body
// on success, so this stays on a plain fetch(), same carve-out
// WebhookClient.WebApp's registerHook documents. It also needs the
// x-requestid header, which apiPost has no way to send.
export async function createOrder(request: CreateOrderRequest, requestId: string): Promise<void> {
  const response = await fetch(ordersBase, {
    method: 'POST',
    headers: { 'X-CSRF': '1', 'Content-Type': 'application/json', 'x-requestid': requestId },
    body: JSON.stringify(request),
  })
  if (!response.ok) {
    throw new Error(`POST ${ordersBase} failed: ${response.status}`)
  }
}
