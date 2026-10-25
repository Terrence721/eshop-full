import { render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { MainLayout } from './MainLayout'

function renderWithRoutes(initialPath: string) {
  const router = createMemoryRouter(
    [
      {
        Component: MainLayout,
        children: [{ path: '/', element: <div>Home content</div> }],
      },
    ],
    { initialEntries: [initialPath] },
  )
  return render(<RouterProvider router={router} />)
}

describe('MainLayout', () => {
  it('renders the header title linking home and the matched child route', async () => {
    mockFetchOnce(401)

    renderWithRoutes('/')

    expect(screen.getByRole('link', { name: 'eShop' })).toHaveAttribute('href', '/')
    expect(await screen.findByText('Home content')).toBeInTheDocument()
  })
})
