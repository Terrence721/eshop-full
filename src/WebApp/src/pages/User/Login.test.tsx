import { render, screen, waitFor } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../../test/mockFetch'
import { mockWindowLocation } from '../../test/mockLocation'
import UserLogin from './Login'

function renderAt(initialPath: string) {
  const router = createMemoryRouter(
    [
      { path: '/user/login', Component: UserLogin },
      { path: '/', element: <div>Home</div> },
      { path: '/cart', element: <div>Cart</div> },
    ],
    { initialEntries: [initialPath] },
  )
  return render(<RouterProvider router={router} />)
}

describe('UserLogin', () => {
  it('shows a signing-in message while loading', () => {
    mockFetchOnce(401)

    renderAt('/user/login')

    expect(screen.getByText('Signing in...')).toBeInTheDocument()
  })

  describe('when signed out', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('navigates to /bff/login with returnUrl, defaulting to /', async () => {
      mockFetchOnce(401)

      renderAt('/user/login')

      await waitFor(() => expect(mockedLocation.location.href).toBe('/bff/login?returnUrl=%2F'))
    })

    it('forwards an explicit returnUrl query param', async () => {
      mockFetchOnce(401)

      renderAt('/user/login?returnUrl=%2Fcart')

      await waitFor(() => expect(mockedLocation.location.href).toBe('/bff/login?returnUrl=%2Fcart'))
    })
  })

  it('returns to returnUrl client-side when already signed in', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    renderAt('/user/login?returnUrl=%2Fcart')

    expect(await screen.findByText('Cart')).toBeInTheDocument()
  })
})
