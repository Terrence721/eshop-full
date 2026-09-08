import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { apiGet, apiPost } from './apiFetch'

describe('apiGet', () => {
  it('resolves with the parsed JSON body on success', async () => {
    mockFetchOnce(200, { hello: 'world' })

    await expect(apiGet<{ hello: string }>('/Home/Index')).resolves.toEqual({ hello: 'world' })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(404)

    await expect(apiGet('/Home/Index', { treatAsNull: [404] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiGet('/Home/Index')).rejects.toThrow('GET /Home/Index failed: 500')
  })

  it('strips the query string from the error message', async () => {
    mockFetchOnce(500)

    await expect(apiGet('/Home/Error?errorId=abc123')).rejects.toThrow('GET /Home/Error failed: 500')
  })
})

describe('apiPost', () => {
  it('sends no body when none is given', async () => {
    const fetchMock = mockFetchOnce(200, { ok: true })

    await apiPost('/Grants/Revoke?clientId=webapp')

    expect(fetchMock).toHaveBeenCalledWith('/Grants/Revoke?clientId=webapp', { method: 'POST' })
  })

  it('sends a JSON body with the correct content-type header when given', async () => {
    const fetchMock = mockFetchOnce(200, { ok: true })

    await apiPost('/Account/Login', { body: { username: 'alice' } })

    expect(fetchMock).toHaveBeenCalledWith('/Account/Login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'alice' }),
    })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(404)

    await expect(apiPost('/Consent/Index', { treatAsNull: [404] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiPost('/Grants/Revoke?clientId=webapp')).rejects.toThrow('POST /Grants/Revoke failed: 500')
  })
})
