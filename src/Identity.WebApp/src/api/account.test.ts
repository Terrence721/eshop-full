import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import {
  buildLogoutFormAction,
  getLoggedOut,
  getLogin,
  getLogout,
  postLogin,
  postLoginCancel,
  postLogout,
  type LoginPostResult,
  type LoginViewModel,
} from './account'

describe('getLogin', () => {
  it('returns the parsed view model on success', async () => {
    const data: LoginViewModel = {
      username: null,
      returnUrl: null,
      rememberLogin: false,
      allowRememberLogin: true,
      enableLocalLogin: true,
      externalProviders: [],
      visibleExternalProviders: [],
      isExternalLoginOnly: false,
      externalLoginScheme: null,
    }
    mockFetchOnce(200, data)

    await expect(getLogin(null)).resolves.toEqual(data)
  })

  it('omits the query string when returnUrl is null', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await getLogin(null)

    expect(fetchMock).toHaveBeenCalledWith('/Account/Login')
  })

  it('URL-encodes returnUrl in the query string when present', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await getLogin('/connect/authorize?client_id=webapp')

    expect(fetchMock).toHaveBeenCalledWith('/Account/Login?returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dwebapp')
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getLogin(null)).rejects.toThrow('GET /Account/Login failed: 500')
  })
})

describe('postLogin', () => {
  it('POSTs the request as a JSON body and returns the parsed result', async () => {
    const result: LoginPostResult = { redirectUrl: '/', isNativeClient: false, viewModel: null, validationError: null }
    const fetchMock = mockFetchOnce(200, result)

    await expect(postLogin({ username: 'alice', password: 'Pass123$', rememberLogin: false, returnUrl: null })).resolves.toEqual(result)

    expect(fetchMock).toHaveBeenCalledWith('/Account/Login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: 'alice', password: 'Pass123$', rememberLogin: false, returnUrl: null }),
    })
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(postLogin({ username: 'alice', password: 'wrong', rememberLogin: false, returnUrl: null })).rejects.toThrow(
      'POST /Account/Login failed: 500',
    )
  })
})

describe('postLoginCancel', () => {
  it('omits the query string when returnUrl is null', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await postLoginCancel(null)

    expect(fetchMock).toHaveBeenCalledWith('/Account/LoginCancel', { method: 'POST' })
  })

  it('URL-encodes returnUrl when present', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await postLoginCancel('/connect/authorize?client_id=webapp')

    expect(fetchMock).toHaveBeenCalledWith('/Account/LoginCancel?returnUrl=%2Fconnect%2Fauthorize%3Fclient_id%3Dwebapp', { method: 'POST' })
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(postLoginCancel(null)).rejects.toThrow('POST /Account/LoginCancel failed: 500')
  })
})

describe('getLogout', () => {
  it('omits the query string when logoutId is null', async () => {
    const fetchMock = mockFetchOnce(200, { logoutId: null, showLogoutPrompt: false })

    await getLogout(null)

    expect(fetchMock).toHaveBeenCalledWith('/Account/Logout')
  })

  it('URL-encodes logoutId when present', async () => {
    const fetchMock = mockFetchOnce(200, { logoutId: 'abc 123', showLogoutPrompt: true })

    await getLogout('abc 123')

    expect(fetchMock).toHaveBeenCalledWith('/Account/Logout?logoutId=abc%20123')
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getLogout(null)).rejects.toThrow('GET /Account/Logout failed: 500')
  })
})

describe('getLoggedOut', () => {
  it('omits the query string when logoutId is null', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await getLoggedOut(null)

    expect(fetchMock).toHaveBeenCalledWith('/Account/LoggedOut')
  })

  it('URL-encodes logoutId when present', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await getLoggedOut('abc 123')

    expect(fetchMock).toHaveBeenCalledWith('/Account/LoggedOut?logoutId=abc%20123')
  })

  it('throws for a non-ok status', async () => {
    mockFetchOnce(500)

    await expect(getLoggedOut(null)).rejects.toThrow('GET /Account/LoggedOut failed: 500')
  })
})

describe('postLogout', () => {
  it('sends logoutId as a query parameter, not a JSON body', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await postLogout('abc 123')

    expect(fetchMock).toHaveBeenCalledWith('/Account/Logout?logoutId=abc%20123', { method: 'POST' })
  })

  it('omits the query string when logoutId is null', async () => {
    const fetchMock = mockFetchOnce(200, {})

    await postLogout(null)

    expect(fetchMock).toHaveBeenCalledWith('/Account/Logout', { method: 'POST' })
  })

  it('throws for a non-ok status -- callers rely on this to fall back to a real <form> POST', async () => {
    mockFetchOnce(500)

    await expect(postLogout('abc123')).rejects.toThrow('POST /Account/Logout failed: 500')
  })
})

describe('buildLogoutFormAction', () => {
  it('matches postLogout\'s own URL shape so the <form> fallback hits the identical endpoint', () => {
    expect(buildLogoutFormAction('abc 123')).toBe('/Account/Logout?logoutId=abc%20123')
    expect(buildLogoutFormAction(null)).toBe('/Account/Logout')
  })
})
