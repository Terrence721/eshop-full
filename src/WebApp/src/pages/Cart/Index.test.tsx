import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchByUrl } from '../../test/mockFetch'
import CartIndex from './Index'

function renderWithRoutes() {
  return render(
    <MemoryRouter>
      <CartIndex />
    </MemoryRouter>,
  )
}

describe('CartIndex', () => {
  it('shows a loading state before the basket fetch resolves', () => {
    mockFetchByUrl({ '/api/basket': { status: 200, body: [] } })

    renderWithRoutes()

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('shows an empty-bag message when the basket has no lines', async () => {
    mockFetchByUrl({ '/api/basket': { status: 200, body: [] } })

    renderWithRoutes()

    expect(await screen.findByText('Your shopping bag is empty.')).toBeInTheDocument()
  })

  it('renders a line per basket item with the resolved catalog details and a running total', async () => {
    mockFetchByUrl({
      '/api/basket': { status: 200, body: [{ productId: 1, quantity: 2 }] },
      '/remoteapi/catalog/api/catalog/items/1': { status: 200, body: { id: 1, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 } },
    })

    renderWithRoutes()

    expect(await screen.findByText('Mug')).toBeInTheDocument()
    expect(screen.getByLabelText('Quantity for Mug')).toHaveValue(2)
    expect(screen.getByText('$10.00')).toBeInTheDocument()
    expect(screen.getByText('Total: $10.00')).toBeInTheDocument()
  })

  it('links to /checkout', async () => {
    mockFetchByUrl({
      '/api/basket': { status: 200, body: [{ productId: 1, quantity: 1 }] },
      '/remoteapi/catalog/api/catalog/items/1': { status: 200, body: { id: 1, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 } },
    })

    renderWithRoutes()

    expect(await screen.findByRole('link', { name: 'Checkout' })).toHaveAttribute('href', '/checkout')
  })

  it('shows an error message when the basket fetch fails', async () => {
    mockFetchByUrl({ '/api/basket': { status: 500 } })

    renderWithRoutes()

    expect(await screen.findByText(/failed: 500/)).toBeInTheDocument()
  })
})
