import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { createOrder, getCardTypes, getOrders, type CreateOrderRequest } from './orders'

describe('getOrders', () => {
  it('resolves with the order summaries', async () => {
    mockFetchOnce(200, [{ orderNumber: 1, date: '2026-01-01', status: 'Submitted', total: 10 }])

    await expect(getOrders()).resolves.toEqual([{ orderNumber: 1, date: '2026-01-01', status: 'Submitted', total: 10 }])
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getOrders()).rejects.toThrow('GET /remoteapi/orders/api/orders failed: 500')
  })
})

describe('getCardTypes', () => {
  it('resolves with the card type list', async () => {
    mockFetchOnce(200, [{ id: 1, name: 'Amex' }])

    await expect(getCardTypes()).resolves.toEqual([{ id: 1, name: 'Amex' }])
  })
})

describe('createOrder', () => {
  const request: CreateOrderRequest = {
    userId: 'alice',
    userName: 'Alice',
    city: 'Redmond',
    street: '1 Microsoft Way',
    state: 'WA',
    country: 'USA',
    zipCode: '98052',
    cardNumber: '1111222233334444',
    cardHolderName: 'TESTUSER',
    cardExpiration: '2030-01-01',
    cardSecurityNumber: '123',
    cardTypeId: 1,
    buyer: 'alice',
    items: [{ productId: 1, productName: 'Mug', unitPrice: 5, oldUnitPrice: 0, quantity: 2, pictureUrl: '/pic/1' }],
  }

  it('sends the request with the X-CSRF and x-requestid headers, resolving on a bare 200', async () => {
    const fetchMock = mockFetchOnce(200)

    await expect(createOrder(request, 'req-123')).resolves.toBeUndefined()

    expect(fetchMock).toHaveBeenCalledWith('/remoteapi/orders/api/orders', {
      method: 'POST',
      headers: { 'X-CSRF': '1', 'Content-Type': 'application/json', 'x-requestid': 'req-123' },
      body: JSON.stringify(request),
    })
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(400)

    await expect(createOrder(request, 'req-123')).rejects.toThrow('POST /remoteapi/orders/api/orders failed: 400')
  })
})
