import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { mockFetchByUrl } from '../../test/mockFetch'
import ItemIndex from './Index'

const mug = { id: 1, name: 'Mug', description: 'A mug', price: 5, catalogTypeId: 1, catalogBrandId: 1 }

function renderAt(itemId: string, overrides: Record<string, { status: number; body?: unknown }> = {}) {
  mockFetchByUrl({
    [`/remoteapi/catalog/api/catalog/items/${itemId}`]: { status: 200, body: mug },
    '/api/basket': { status: 200, body: [] },
    ...overrides,
  })
  const router = createMemoryRouter([{ path: '/item/:itemId', Component: ItemIndex }], {
    initialEntries: [`/item/${itemId}`],
  })
  return render(<RouterProvider router={router} />)
}

describe('ItemIndex', () => {
  it('shows a loading state before the fetch resolves', () => {
    renderAt('1')

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('renders the item once loaded, with the current bag count', async () => {
    renderAt('1', { '/api/basket': { status: 200, body: [{ productId: 1, quantity: 2 }] } })

    expect(await screen.findByText('Mug')).toBeInTheDocument()
    expect(screen.getByText('A mug')).toBeInTheDocument()
    expect(screen.getByText('$5.00')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Mug' })).toHaveAttribute('src', '/remoteapi/catalog/api/catalog/items/1/pic')
    expect(screen.getByText('2 in your shopping bag')).toBeInTheDocument()
  })

  it('shows a not-found message when the item does not exist', async () => {
    renderAt('999', { '/remoteapi/catalog/api/catalog/items/999': { status: 404 } })

    expect(await screen.findByText(/couldn't find the item/)).toBeInTheDocument()
  })

  it('shows an error message when the fetch fails', async () => {
    renderAt('1', { '/remoteapi/catalog/api/catalog/items/1': { status: 500 } })

    expect(await screen.findByText(/failed: 500/)).toBeInTheDocument()
  })

  it('clicking Add to shopping bag adds the item and updates the bag count', async () => {
    const fetchMock = mockFetchByUrl({
      '/remoteapi/catalog/api/catalog/items/1': { status: 200, body: mug },
      '/api/basket': { status: 200, body: [] },
    })
    const router = createMemoryRouter([{ path: '/item/:itemId', Component: ItemIndex }], { initialEntries: ['/item/1'] })
    render(<RouterProvider router={router} />)
    const user = userEvent.setup()

    fetchMock.mockResolvedValueOnce({ ok: true, status: 200, json: vi.fn().mockResolvedValue([{ productId: 1, quantity: 1 }]) })
    await user.click(await screen.findByRole('button', { name: 'Add to shopping bag' }))

    expect(await screen.findByText('1 in your shopping bag')).toBeInTheDocument()
  })
})
