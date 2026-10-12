import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getReceivedMessages } from './received'

describe('getReceivedMessages', () => {
  it('resolves with the list of received webhook messages', async () => {
    mockFetchOnce(200, [{ when: '2026-01-01T00:00:00Z', data: '{"orderId":1}', token: 'abc' }])

    await expect(getReceivedMessages()).resolves.toEqual([{ when: '2026-01-01T00:00:00Z', data: '{"orderId":1}', token: 'abc' }])
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getReceivedMessages()).rejects.toThrow('GET /api/received failed: 500')
  })
})
