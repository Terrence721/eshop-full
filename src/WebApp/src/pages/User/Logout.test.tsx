import { render, screen, waitFor } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../../test/mockFetch'
import { mockWindowLocation } from '../../test/mockLocation'
import UserLogout from './Logout'

function renderAt(initialPath: string) {
  const router = createMemoryRouter(
    [
      { path: '/user/logout', Component: UserLogout },
      { path: '/', element: <div>Home</div> },
    ],
    { initialEntries: [initialPath] },
  )
  return render(<RouterProvider router={router} />)
}

describe('UserLogout', () => {
  it('shows a signing-out message while loading', () => {
    mockFetchOnce(401)

    renderAt('/user/logout')

    expect(screen.getByText('Signing out...')).toBeInTheDocument()
  })

  describe('when a logout_url claim is present', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('navigates to the real front-channel sign-out URL', async () => {
      mockFetchOnce(200, [
        { type: 'name', value: 'alice' },
        { type: 'bff:logout_url', value: '/bff/logout?sid=abc123' },
      ])

      renderAt('/user/logout')

      await waitFor(() => expect(mockedLocation.location.href).toBe('/bff/logout?sid=abc123'))
    })
  })

  it('navigates home when there is no session to sign out of', async () => {
    mockFetchOnce(401)

    renderAt('/user/logout')

    expect(await screen.findByText('Home')).toBeInTheDocument()
  })
})
