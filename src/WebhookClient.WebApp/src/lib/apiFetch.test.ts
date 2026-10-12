import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { apiGet, apiPost } from './apiFetch'

describe('apiGet', () => {
  it('resolves with the parsed JSON body on success', async () => {
    mockFetchOnce(200, { name: 'alice' })

    await expect(apiGet<{ name: string }>('/bff/user')).resolves.toEqual({ name: 'alice' })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(401)

    await expect(apiGet('/bff/user', { treatAsNull: [401] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiGet('/api/hooks')).rejects.toThrow('GET /api/hooks failed: 500')
  })

  it('strips the query string from the error message', async () => {
    mockFetchOnce(500)

    await expect(apiGet('/api/hooks?token=abc123')).rejects.toThrow('GET /api/hooks failed: 500')
  })
})

describe('apiPost', () => {
  it('sends no body when none is given', async () => {
    const fetchMock = mockFetchOnce(200, { ok: true })

    await apiPost('/logout')

    expect(fetchMock).toHaveBeenCalledWith('/logout', { method: 'POST' })
  })

  it('sends a JSON body with the correct content-type header when given', async () => {
    const fetchMock = mockFetchOnce(200, { ok: true })

    await apiPost('/api/hooks', { body: { event: 'OrderPaid' } })

    expect(fetchMock).toHaveBeenCalledWith('/api/hooks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ event: 'OrderPaid' }),
    })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(404)

    await expect(apiPost('/api/hooks', { treatAsNull: [404] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiPost('/api/hooks')).rejects.toThrow('POST /api/hooks failed: 500')
  })
})
