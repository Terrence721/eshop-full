import { render, screen, waitFor } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { mockWindowLocation } from '../test/mockLocation'
import { RequireAuth } from './RequireAuth'

function renderAt(initialPath: string) {
  const router = createMemoryRouter(
    [
      {
        Component: RequireAuth,
        children: [{ path: '/cart', element: <div>Protected content</div> }],
      },
    ],
    { initialEntries: [initialPath] },
  )
  return render(<RouterProvider router={router} />)
}

describe('RequireAuth', () => {
  it('renders nothing while loading', () => {
    mockFetchOnce(401)

    const { container } = renderAt('/cart')

    expect(container).toBeEmptyDOMElement()
  })

  it('renders the protected route when signed in', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    renderAt('/cart')

    expect(await screen.findByText('Protected content')).toBeInTheDocument()
  })

  describe('when signed out', () => {
    let mockedLocation: ReturnType<typeof mockWindowLocation>

    beforeEach(() => {
      mockedLocation = mockWindowLocation()
    })

    afterEach(() => {
      mockedLocation.restore()
    })

    it('redirects to /user/login with the current path as returnUrl', async () => {
      mockFetchOnce(401)

      renderAt('/cart?foo=bar')

      await waitFor(() => expect(mockedLocation.location.href).toBe('/user/login?returnUrl=%2Fcart%3Ffoo%3Dbar'))
    })
  })
})
