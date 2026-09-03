import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { getConsent, postConsent, type ConsentViewModel } from './consent'

function scopelessViewModel(): ConsentViewModel {
  return {
    button: null,
    scopesConsented: null,
    rememberConsent: true,
    returnUrl: '/connect/authorize/callback',
    description: null,
    clientName: 'WebApp Client',
    clientUrl: null,
    clientLogoUrl: null,
    allowRememberConsent: true,
    identityScopes: [],
    apiScopes: [],
  }
}

describe('getConsent', () => {
  it('returns the parsed view model on success', async () => {
    const data = scopelessViewModel()
    mockFetchOnce(200, data)

    await expect(getConsent('/connect/authorize/callback')).resolves.toEqual(data)
  })

  it('treats a 404 as null, not an error -- no matching authorization request', async () => {
    mockFetchOnce(404)

    await expect(getConsent('/connect/authorize/callback')).resolves.toBeNull()
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getConsent('/connect/authorize/callback')).rejects.toThrow('GET /Consent/Index failed: 500')
  })

  it('URL-encodes returnUrl in the query string', async () => {
    const fetchMock = mockFetchOnce(200, scopelessViewModel())

    await getConsent('/connect/authorize/callback?client_id=webapp')

    expect(fetchMock).toHaveBeenCalledWith('/Consent/Index?returnUrl=%2Fconnect%2Fauthorize%2Fcallback%3Fclient_id%3Dwebapp')
  })
})

describe('postConsent', () => {
  it('POSTs the request as a JSON body and returns the parsed result on success', async () => {
    const result = { redirectUrl: '/connect/authorize/callback', isNativeClient: false, validationError: null, viewModel: null }
    const fetchMock = mockFetchOnce(200, result)
    const request = {
      button: 'yes' as const,
      scopesConsented: ['openid', 'profile'],
      rememberConsent: true,
      returnUrl: '/connect/authorize/callback',
      description: null,
    }

    await expect(postConsent(request)).resolves.toEqual(result)

    expect(fetchMock).toHaveBeenCalledWith('/Consent/Index', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    })
  })

  it('treats a 404 as null, not an error -- same as GET, no matching authorization request', async () => {
    mockFetchOnce(404)

    await expect(
      postConsent({ button: 'no', scopesConsented: [], rememberConsent: false, returnUrl: '/connect/authorize/callback', description: null }),
    ).resolves.toBeNull()
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(
      postConsent({ button: 'yes', scopesConsented: [], rememberConsent: false, returnUrl: '/connect/authorize/callback', description: null }),
    ).rejects.toThrow('POST /Consent/Index failed: 500')
  })
})
