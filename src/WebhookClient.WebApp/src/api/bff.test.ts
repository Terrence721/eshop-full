import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getCurrentUser } from './bff'

describe('getCurrentUser', () => {
  it('resolves with the claims array on success', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    await expect(getCurrentUser()).resolves.toEqual([{ type: 'name', value: 'alice' }])
  })

  it('resolves with null when unauthenticated', async () => {
    mockFetchOnce(401)

    await expect(getCurrentUser()).resolves.toBeNull()
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getCurrentUser()).rejects.toThrow('GET /bff/user failed: 500')
  })
})
