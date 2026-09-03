import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getHomeError, getHomeIndex, type HomeIndex } from './home'

describe('getHomeIndex', () => {
  it('returns the parsed view model on success', async () => {
    const data: HomeIndex = {
      version: '8.0.6',
      wellKnownConfigurationUrl: '/.well-known/openid-configuration',
      diagnosticsUrl: '/Diagnostics/Index',
      grantsUrl: '/Grants/Index',
    }
    mockFetchOnce(200, data)

    await expect(getHomeIndex()).resolves.toEqual(data)
  })

  it('treats a 404 as null, not an error -- this action is disabled outside Development', async () => {
    mockFetchOnce(404)

    await expect(getHomeIndex()).resolves.toBeNull()
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getHomeIndex()).rejects.toThrow('GET /Home/Index failed: 500')
  })

  it('requests the real endpoint', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await getHomeIndex()

    expect(fetchMock).toHaveBeenCalledWith('/Home/Index')
  })
})

describe('getHomeError', () => {
  it('returns the parsed error payload', async () => {
    mockFetchOnce(200, { error: { errorType: 'invalid_request' } })

    await expect(getHomeError('abc123')).resolves.toEqual({ error: { errorType: 'invalid_request' } })
  })

  it('URL-encodes the errorId in the query string', async () => {
    const fetchMock = mockFetchOnce(200, { error: null })

    await getHomeError('a b/c')

    expect(fetchMock).toHaveBeenCalledWith('/Home/Error?errorId=a%20b%2Fc')
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getHomeError('abc123')).rejects.toThrow('GET /Home/Error failed: 500')
  })
})
