import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { clearBasket, getBasket, updateBasket } from './basket'

describe('getBasket', () => {
  it('resolves with the basket lines', async () => {
    mockFetchOnce(200, [{ productId: 1, quantity: 2 }])

    await expect(getBasket()).resolves.toEqual([{ productId: 1, quantity: 2 }])
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getBasket()).rejects.toThrow('GET /api/basket failed: 500')
  })
})

describe('updateBasket', () => {
  it('sends the bare array as the request body, not wrapped in an envelope', async () => {
    const fetchMock = mockFetchOnce(200, [{ productId: 1, quantity: 2 }])

    await updateBasket([{ productId: 1, quantity: 2 }])

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', {
      method: 'PUT',
      headers: { 'X-CSRF': '1', 'Content-Type': 'application/json' },
      body: JSON.stringify([{ productId: 1, quantity: 2 }]),
    })
  })

  it('resolves with the updated basket lines', async () => {
    mockFetchOnce(200, [{ productId: 1, quantity: 3 }])

    await expect(updateBasket([{ productId: 1, quantity: 3 }])).resolves.toEqual([{ productId: 1, quantity: 3 }])
  })
})

describe('clearBasket', () => {
  it('resolves to null on a 204', async () => {
    const fetchMock = mockFetchOnce(204)

    await expect(clearBasket()).resolves.toBeNull()

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', { method: 'DELETE', headers: { 'X-CSRF': '1' } })
  })
})
