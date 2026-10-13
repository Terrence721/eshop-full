import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import type { CatalogItem } from '../api/catalog'
import { CatalogItemCard } from './CatalogItemCard'

const item: CatalogItem = { id: 7, name: 'Mug', description: null, price: 9.5, catalogTypeId: 1, catalogBrandId: 1 }

describe('CatalogItemCard', () => {
  it('links to the item detail page', () => {
    render(
      <MemoryRouter>
        <CatalogItemCard item={item} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link')).toHaveAttribute('href', '/item/7')
  })

  it('shows the picture, name, and formatted price', () => {
    render(
      <MemoryRouter>
        <CatalogItemCard item={item} />
      </MemoryRouter>,
    )

    expect(screen.getByRole('img', { name: 'Mug' })).toHaveAttribute('src', '/remoteapi/catalog/items/7/pic')
    expect(screen.getByText('Mug')).toBeInTheDocument()
    expect(screen.getByText('$9.50')).toBeInTheDocument()
  })
})
