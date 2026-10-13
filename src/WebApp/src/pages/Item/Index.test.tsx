import { render, screen } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../../test/mockFetch'
import ItemIndex from './Index'

function renderAt(itemId: string) {
  const router = createMemoryRouter([{ path: '/item/:itemId', Component: ItemIndex }], {
    initialEntries: [`/item/${itemId}`],
  })
  return render(<RouterProvider router={router} />)
}

describe('ItemIndex', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchOnce(200, { id: 1, name: 'Mug', description: 'A mug', price: 5, catalogTypeId: 1, catalogBrandId: 1 })

    renderAt('1')

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('renders the item once loaded', async () => {
    mockFetchOnce(200, { id: 1, name: 'Mug', description: 'A mug', price: 5, catalogTypeId: 1, catalogBrandId: 1 })

    renderAt('1')

    expect(await screen.findByText('Mug')).toBeInTheDocument()
    expect(screen.getByText('A mug')).toBeInTheDocument()
    expect(screen.getByText('$5.00')).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Mug' })).toHaveAttribute('src', '/remoteapi/catalog/items/1/pic')
  })

  it('shows a not-found message when the item does not exist', async () => {
    mockFetchOnce(404)

    renderAt('999')

    expect(await screen.findByText(/couldn't find the item/)).toBeInTheDocument()
  })

  it('shows an error message when the fetch fails', async () => {
    mockFetchOnce(500)

    renderAt('1')

    expect(await screen.findByText(/failed: 500/)).toBeInTheDocument()
  })
})
