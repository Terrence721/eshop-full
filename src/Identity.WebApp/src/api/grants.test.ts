import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getGrants, revokeGrant, type GrantsViewModel } from './grants'

function grantsWithOneClient(): GrantsViewModel {
  return {
    grants: [
      {
        clientId: 'webapp',
        clientName: 'WebApp Client',
        clientUrl: 'https://localhost:7100',
        clientLogoUrl: null,
        description: null,
        created: '2026-09-03T18:13:18.3226444Z',
        expires: null,
        identityGrantNames: ['Your user identifier', 'User profile'],
        apiGrantNames: [],
      },
    ],
  }
}

describe('getGrants', () => {
  it('returns the parsed grants list on success', async () => {
    const data = grantsWithOneClient()
    mockFetchOnce(200, data)

    await expect(getGrants()).resolves.toEqual(data)
  })

  it('throws for a non-ok status -- unlike Diagnostics/Consent, there is no 404-as-null case here', async () => {
    mockFetchOnce(404)

    await expect(getGrants()).rejects.toThrow('GET /Grants/Index failed: 404')
  })

  it('requests the real endpoint', async () => {
    const fetchMock = mockFetchOnce(200, { grants: [] })

    await getGrants()

    expect(fetchMock).toHaveBeenCalledWith('/Grants/Index')
  })
})

describe('revokeGrant', () => {
  it('sends clientId as a query parameter, not a JSON body, and returns the updated list', async () => {
    const result: GrantsViewModel = { grants: [] }
    const fetchMock = mockFetchOnce(200, result)

    await expect(revokeGrant('webapp')).resolves.toEqual(result)

    expect(fetchMock).toHaveBeenCalledWith('/Grants/Revoke?clientId=webapp', { method: 'POST' })
  })

  it('URL-encodes clientId', async () => {
    const fetchMock = mockFetchOnce(200, { grants: [] })

    await revokeGrant('a client/id')

    expect(fetchMock).toHaveBeenCalledWith('/Grants/Revoke?clientId=a%20client%2Fid', { method: 'POST' })
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(revokeGrant('webapp')).rejects.toThrow('POST /Grants/Revoke failed: 500')
  })
})
