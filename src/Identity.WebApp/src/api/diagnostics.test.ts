import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getDiagnostics, type DiagnosticsViewModel } from './diagnostics'

describe('getDiagnostics', () => {
  it('returns the parsed view model on success', async () => {
    const data: DiagnosticsViewModel = {
      claims: [{ type: 'sub', value: 'f20456ba-3b16-47f6-b31d-01ae19a7502d' }],
      properties: { session_id: '3D38660D', '.expires': null },
      clients: [],
    }
    mockFetchOnce(200, data)

    await expect(getDiagnostics()).resolves.toEqual(data)
  })

  it('treats a 404 as null, not an error -- the real loopback-only gate', async () => {
    mockFetchOnce(404)

    await expect(getDiagnostics()).resolves.toBeNull()
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getDiagnostics()).rejects.toThrow('GET /Diagnostics/Index failed: 500')
  })

  it('requests the real endpoint', async () => {
    const fetchMock = mockFetchOnce(200, { claims: [], properties: {}, clients: [] })

    await getDiagnostics()

    expect(fetchMock).toHaveBeenCalledWith('/Diagnostics/Index')
  })
})
