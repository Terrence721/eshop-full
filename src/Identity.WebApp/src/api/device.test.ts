import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { captureUserCode, getDeviceIndex, postDeviceCallback, type DeviceAuthorizationViewModel } from './device'

function deviceViewModel(): DeviceAuthorizationViewModel {
  return {
    button: null,
    scopesConsented: [],
    rememberConsent: true,
    returnUrl: null,
    description: null,
    clientName: 'SCRATCH-DIAGNOSTIC Device Test Client',
    clientUrl: null,
    clientLogoUrl: null,
    allowRememberConsent: true,
    identityScopes: [],
    apiScopes: [],
    userCode: '593995389',
    confirmUserCode: true,
  }
}

describe('getDeviceIndex', () => {
  it('requests the bare endpoint when no userCode is given', async () => {
    const fetchMock = mockFetchOnce(200, { needsUserCode: true, viewModel: null })

    await getDeviceIndex()

    expect(fetchMock).toHaveBeenCalledWith('/Device/Index')
  })

  it('returns needsUserCode: true when no code is present yet', async () => {
    mockFetchOnce(200, { needsUserCode: true, viewModel: null })

    await expect(getDeviceIndex()).resolves.toEqual({ needsUserCode: true, viewModel: null })
  })

  it('URL-encodes userCode when given and returns the real view model', async () => {
    const vm = deviceViewModel()
    const fetchMock = mockFetchOnce(200, { needsUserCode: false, viewModel: vm })

    await expect(getDeviceIndex('abc 123')).resolves.toEqual({ needsUserCode: false, viewModel: vm })
    expect(fetchMock).toHaveBeenCalledWith('/Device/Index?userCode=abc%20123')
  })

  it('treats a 404 as null, not an error -- a missing/expired code, no third state to represent', async () => {
    mockFetchOnce(404)

    await expect(getDeviceIndex('bogus')).resolves.toBeNull()
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getDeviceIndex()).rejects.toThrow('GET /Device/Index failed: 500')
  })
})

describe('captureUserCode', () => {
  it('sends userCode as a query parameter, not a JSON body', async () => {
    const vm = deviceViewModel()
    const fetchMock = mockFetchOnce(200, vm)

    await expect(captureUserCode('593995389')).resolves.toEqual(vm)

    expect(fetchMock).toHaveBeenCalledWith('/Device/UserCodeCapture?userCode=593995389', { method: 'POST' })
  })

  it('URL-encodes userCode', async () => {
    const fetchMock = mockFetchOnce(200, deviceViewModel())

    await captureUserCode('a code/1')

    expect(fetchMock).toHaveBeenCalledWith('/Device/UserCodeCapture?userCode=a%20code%2F1', { method: 'POST' })
  })

  it('treats a 404 as null, not an error -- an invalid code', async () => {
    mockFetchOnce(404)

    await expect(captureUserCode('bogus')).resolves.toBeNull()
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(captureUserCode('593995389')).rejects.toThrow('POST /Device/UserCodeCapture failed: 500')
  })
})

describe('postDeviceCallback', () => {
  const request = {
    userCode: '593995389',
    button: 'yes' as const,
    scopesConsented: ['openid', 'profile'],
    rememberConsent: true,
    description: null,
  }

  it('204 maps to the success outcome, with no body parsed', async () => {
    const fetchMock = mockFetchOnce(204)

    await expect(postDeviceCallback(request)).resolves.toEqual({ outcome: 'success' })

    expect(fetchMock).toHaveBeenCalledWith('/Device/Callback', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    })
  })

  it('404 maps to the notFound outcome -- the device-flow authorization vanished', async () => {
    mockFetchOnce(404)

    await expect(postDeviceCallback(request)).resolves.toEqual({ outcome: 'notFound' })
  })

  it('200 maps to the redisplay outcome, carrying the real validation error and view model', async () => {
    const result = { validationError: 'You must pick at least one permission', viewModel: deviceViewModel() }
    mockFetchOnce(200, result)

    await expect(postDeviceCallback({ ...request, scopesConsented: [] })).resolves.toEqual({ outcome: 'redisplay', result })
  })

  it('throws for any other non-ok status', async () => {
    mockFetchOnce(500)

    await expect(postDeviceCallback(request)).rejects.toThrow('POST /Device/Callback failed: 500')
  })
})
