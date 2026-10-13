import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider, useLocation } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchByUrl } from '../test/mockFetch'
import { CatalogFilters } from './CatalogFilters'

function LocationDisplay() {
  const location = useLocation()
  return <div data-testid="location">{location.pathname + location.search}</div>
}

function renderAt(initialPath: string) {
  const router = createMemoryRouter(
    [
      {
        path: '/',
        Component: () => (
          <>
            <CatalogFilters />
            <LocationDisplay />
          </>
        ),
      },
    ],
    { initialEntries: [initialPath] },
  )
  return render(<RouterProvider router={router} />)
}

function stubBrandsAndTypes() {
  return mockFetchByUrl({
    '/remoteapi/catalog/api/catalog/catalogbrands': { status: 200, body: [{ id: 1, brand: 'Adventure Works' }] },
    '/remoteapi/catalog/api/catalog/catalogtypes': { status: 200, body: [{ id: 2, type: 'Mugs' }] },
  })
}

describe('CatalogFilters', () => {
  it('renders the fetched brand and type options alongside "All"', async () => {
    stubBrandsAndTypes()

    renderAt('/')

    expect(await screen.findByRole('option', { name: 'Adventure Works' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Mugs' })).toBeInTheDocument()
    expect(screen.getAllByRole('option', { name: 'All' })).toHaveLength(2)
  })

  it('selecting a brand navigates with ?brand= and resets the page', async () => {
    stubBrandsAndTypes()
    const user = userEvent.setup()

    renderAt('/?page=3')

    await user.selectOptions(screen.getByLabelText('Brand'), await screen.findByRole('option', { name: 'Adventure Works' }))

    expect(screen.getByTestId('location')).toHaveTextContent('/?brand=1')
  })

  it('selecting type preserves an existing brand filter', async () => {
    stubBrandsAndTypes()
    const user = userEvent.setup()

    renderAt('/?brand=1')

    await user.selectOptions(screen.getByLabelText('Type'), await screen.findByRole('option', { name: 'Mugs' }))

    expect(screen.getByTestId('location')).toHaveTextContent('/?brand=1&type=2')
  })

  it('choosing "All" clears the filter', async () => {
    stubBrandsAndTypes()
    const user = userEvent.setup()

    renderAt('/?brand=1')
    await screen.findByRole('option', { name: 'Adventure Works' })

    await user.selectOptions(screen.getByLabelText('Brand'), 'All')

    expect(screen.getByTestId('location')).not.toHaveTextContent('brand=')
  })
})
