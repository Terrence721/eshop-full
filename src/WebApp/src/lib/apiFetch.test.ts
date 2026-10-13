import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { apiDelete, apiGet, apiPost, apiPut } from './apiFetch'

describe('apiGet', () => {
  it('resolves with the parsed JSON body on success', async () => {
    mockFetchOnce(200, { name: 'alice' })

    await expect(apiGet<{ name: string }>('/remoteapi/catalog/api/catalog/items/1')).resolves.toEqual({ name: 'alice' })
  })

  it('sends the X-CSRF header', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await apiGet('/remoteapi/catalog/api/catalog/items/1')

    expect(fetchMock).toHaveBeenCalledWith('/remoteapi/catalog/api/catalog/items/1', { headers: { 'X-CSRF': '1' } })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(404)

    await expect(apiGet('/remoteapi/catalog/api/catalog/items/1', { treatAsNull: [404] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiGet('/remoteapi/catalog/api/catalog/items/1')).rejects.toThrow('GET /remoteapi/catalog/api/catalog/items/1 failed: 500')
  })

  it('strips the query string from the error message', async () => {
    mockFetchOnce(500)

    await expect(apiGet('/remoteapi/catalog/api/catalog/items?page=2')).rejects.toThrow('GET /remoteapi/catalog/api/catalog/items failed: 500')
  })
})

describe('apiPost', () => {
  it('sends the X-CSRF header with no body when none is given', async () => {
    const fetchMock = mockFetchOnce(200, { ok: true })

    await apiPost('/api/basket')

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', { method: 'POST', headers: { 'X-CSRF': '1' } })
  })

  it('sends a JSON body with the content-type and X-CSRF headers when given', async () => {
    const fetchMock = mockFetchOnce(200, { ok: true })

    await apiPost('/remoteapi/orders/api/orders', { body: { items: [] } })

    expect(fetchMock).toHaveBeenCalledWith('/remoteapi/orders/api/orders', {
      method: 'POST',
      headers: { 'X-CSRF': '1', 'Content-Type': 'application/json' },
      body: JSON.stringify({ items: [] }),
    })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(404)

    await expect(apiPost('/remoteapi/orders/api/orders', { treatAsNull: [404] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiPost('/remoteapi/orders/api/orders')).rejects.toThrow('POST /remoteapi/orders/api/orders failed: 500')
  })
})

describe('apiPut', () => {
  it('sends a JSON body with the content-type and X-CSRF headers', async () => {
    const fetchMock = mockFetchOnce(200, { items: [] })

    await apiPut('/api/basket', { body: { items: [{ productId: 1, quantity: 2 }] } })

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', {
      method: 'PUT',
      headers: { 'X-CSRF': '1', 'Content-Type': 'application/json' },
      body: JSON.stringify({ items: [{ productId: 1, quantity: 2 }] }),
    })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(401)

    await expect(apiPut('/api/basket', { body: {}, treatAsNull: [401] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiPut('/api/basket', { body: {} })).rejects.toThrow('PUT /api/basket failed: 500')
  })
})

describe('apiDelete', () => {
  it('sends the X-CSRF header and resolves to null on a 204', async () => {
    const fetchMock = mockFetchOnce(204)

    await expect(apiDelete('/api/basket')).resolves.toBeNull()

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', { method: 'DELETE', headers: { 'X-CSRF': '1' } })
  })

  it('returns null for a status listed in treatAsNull', async () => {
    mockFetchOnce(401)

    await expect(apiDelete('/api/basket', { treatAsNull: [401] })).resolves.toBeNull()
  })

  it('throws for a non-ok status not listed in treatAsNull', async () => {
    mockFetchOnce(500)

    await expect(apiDelete('/api/basket')).rejects.toThrow('DELETE /api/basket failed: 500')
  })
})
