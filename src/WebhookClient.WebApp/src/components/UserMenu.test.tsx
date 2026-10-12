import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { mockWindowLocation } from '../test/mockLocation'
import { UserMenu } from './UserMenu'

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <UserMenu />
    </MemoryRouter>,
  )
}

describe('UserMenu', () => {
  it('renders nothing while loading', () => {
    mockFetchOnce(401)

    const { container } = renderAt('/')

    expect(container).toBeEmptyDOMElement()
  })

  it('shows a Log in button when signed out', async () => {
    mockFetchOnce(401)

    renderAt('/')

    expect(await screen.findByRole('button', { name: 'Log in' })).toBeInTheDocument()
  })

  it('shows the user name and a Log out button when signed in', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    renderAt('/')

    expect(await screen.findByText('alice')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Log out' })).toBeInTheDocument()
  })

  describe('logging in', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('navigates to /bff/login with the current path as returnUrl', async () => {
      mockFetchOnce(401)
      const user = userEvent.setup()

      renderAt('/add-webhook?foo=bar')

      await user.click(await screen.findByRole('button', { name: 'Log in' }))

      expect(mockedLocation.location.href).toBe('/bff/login?returnUrl=%2Fadd-webhook%3Ffoo%3Dbar')
    })
  })
})
