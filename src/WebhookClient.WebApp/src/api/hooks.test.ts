import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getDefaultToken, getRegisteredHooks, registerHook } from './hooks'

describe('getRegisteredHooks', () => {
  it('resolves with the list of registered webhooks', async () => {
    mockFetchOnce(200, [{ date: '2026-01-01T00:00:00Z', destUrl: 'https://example.com/hook', token: 'abc' }])

    await expect(getRegisteredHooks()).resolves.toEqual([
      { date: '2026-01-01T00:00:00Z', destUrl: 'https://example.com/hook', token: 'abc' },
    ])
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getRegisteredHooks()).rejects.toThrow('GET /api/hooks failed: 500')
  })
})

describe('registerHook', () => {
  it('posts just the token as JSON and resolves with ok:true on success', async () => {
    const fetchMock = mockFetchOnce(200)

    await expect(registerHook('abc')).resolves.toEqual({ ok: true, status: 200 })

    expect(fetchMock).toHaveBeenCalledWith('/api/hooks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token: 'abc' }),
    })
  })

  it('resolves with ok:false and the status when rejected, without throwing', async () => {
    mockFetchOnce(400)

    await expect(registerHook('')).resolves.toEqual({ ok: false, status: 400 })
  })
})

describe('getDefaultToken', () => {
  it('resolves with the configured default token', async () => {
    mockFetchOnce(200, { token: '6168DB8D-DC58-4094-AF24-483278923590' })

    await expect(getDefaultToken()).resolves.toEqual({ token: '6168DB8D-DC58-4094-AF24-483278923590' })
  })

  it('throws for an unexpected failure status', async () => {
    mockFetchOnce(500)

    await expect(getDefaultToken()).rejects.toThrow('GET /api/hooks/default-token failed: 500')
  })
})
