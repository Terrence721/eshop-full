import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getOrders } from './orders'

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
