import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { MemoryRouter } from 'react-router'
import { mockFetchOnce, mockFetchSequence } from '../../test/mockFetch'
import { mockWindowLocation } from '../../test/mockLocation'
import type { LoginViewModel } from '../../api/account'
import LoginPage from './Login'

function baseViewModel(overrides: Partial<LoginViewModel> = {}): LoginViewModel {
  return {
    username: null,
    returnUrl: '/connect/authorize/callback',
    rememberLogin: false,
    allowRememberLogin: true,
    enableLocalLogin: true,
    externalProviders: [],
    visibleExternalProviders: [],
    isExternalLoginOnly: false,
    externalLoginScheme: null,
    ...overrides,
  }
}

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <LoginPage />
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchOnce(200, baseViewModel())

    renderAt('/Account/Login')

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('shows a load error message when the initial fetch fails', async () => {
    mockFetchOnce(500)

    renderAt('/Account/Login')

    expect(await screen.findByText('Could not load the login page: GET /Account/Login failed: 500')).toBeInTheDocument()
  })

  it('renders the local login form with username, password, and a Remember me checkbox', async () => {
    mockFetchOnce(200, baseViewModel({ allowRememberLogin: true }))

    renderAt('/Account/Login')

    expect(await screen.findByLabelText('Username')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByLabelText('Remember me')).toBeInTheDocument()
  })

  it('omits the Remember me checkbox when the client disallows it', async () => {
    mockFetchOnce(200, baseViewModel({ allowRememberLogin: false }))

    renderAt('/Account/Login')

    await screen.findByLabelText('Username')
    expect(screen.queryByLabelText('Remember me')).not.toBeInTheDocument()
  })

  it('renders visible external providers as links to the challenge URL', async () => {
    mockFetchOnce(
      200,
      baseViewModel({
        enableLocalLogin: false,
        visibleExternalProviders: [{ displayName: 'Google', authenticationScheme: 'Google' }],
      }),
    )

    renderAt('/Account/Login')

    const link = await screen.findByRole('link', { name: 'Google' })
    expect(link).toHaveAttribute('href', '/External/Challenge?scheme=Google&returnUrl=%2Fconnect%2Fauthorize%2Fcallback')
  })

  it('shows "no login method" when neither local login nor external providers are available', async () => {
    mockFetchOnce(200, baseViewModel({ enableLocalLogin: false, visibleExternalProviders: [] }))

    renderAt('/Account/Login')

    expect(await screen.findByText('No login method is available.')).toBeInTheDocument()
  })

  describe('external-login-only redirect', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('redirects immediately when the client restricts login to one external IdP', async () => {
      mockFetchOnce(200, baseViewModel({ isExternalLoginOnly: true, externalLoginScheme: 'Google' }))

      renderAt('/Account/Login')

      expect(await screen.findByText('Redirecting to sign-in...')).toBeInTheDocument()
      expect(mockedLocation.location.href).toBe('/External/Challenge?scheme=Google&returnUrl=%2Fconnect%2Fauthorize%2Fcallback')
    })
  })

  describe('submitting the form', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('posts the entered credentials and navigates to the real redirectUrl on success', async () => {
      const user = userEvent.setup()
      const fetchMock = mockFetchSequence(
        { status: 200, body: baseViewModel() },
        { status: 200, body: { redirectUrl: '/connect/authorize/callback', isNativeClient: false, viewModel: null, validationError: null } },
      )

      renderAt('/Account/Login?returnUrl=%2Fconnect%2Fauthorize%2Fcallback')

      await user.type(await screen.findByLabelText('Username'), 'alice')
      await user.type(screen.getByLabelText('Password'), 'Pass123$')
      await user.click(screen.getByRole('button', { name: 'Login' }))

      expect(mockedLocation.location.href).toBe('/connect/authorize/callback')
      expect(fetchMock).toHaveBeenLastCalledWith('/Account/Login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: 'alice', password: 'Pass123$', rememberLogin: false, returnUrl: '/connect/authorize/callback' }),
      })
    })

    it('shows the real validation error and redisplays the form without navigating', async () => {
      const user = userEvent.setup()
      mockFetchSequence(
        { status: 200, body: baseViewModel() },
        { status: 200, body: { redirectUrl: null, isNativeClient: false, viewModel: baseViewModel(), validationError: 'Invalid username or password.' } },
      )

      renderAt('/Account/Login?returnUrl=%2Fconnect%2Fauthorize%2Fcallback')

      await user.type(await screen.findByLabelText('Username'), 'alice')
      await user.type(screen.getByLabelText('Password'), 'wrong')
      await user.click(screen.getByRole('button', { name: 'Login' }))

      expect(await screen.findByRole('alert')).toHaveTextContent('Invalid username or password.')
      expect(mockedLocation.location.href).toBe('')
    })
  })

  describe('cancelling', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('posts the cancellation and navigates to the real redirectUrl', async () => {
      const user = userEvent.setup()
      mockFetchSequence(
        { status: 200, body: baseViewModel() },
        { status: 200, body: { redirectUrl: '/connect/authorize/callback', isNativeClient: false, viewModel: null, validationError: null } },
      )

      renderAt('/Account/Login')

      await user.click(await screen.findByRole('button', { name: 'Cancel' }))

      expect(mockedLocation.location.href).toBe('/connect/authorize/callback')
    })
  })
})
