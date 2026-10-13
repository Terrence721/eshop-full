import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import type { CatalogItem } from '../api/catalog'
import { CartLineItem } from './CartLineItem'

const item: CatalogItem = { id: 7, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 }

describe('CartLineItem', () => {
  it('shows the picture, name, unit price, quantity, and line total', () => {
    render(<CartLineItem item={item} quantity={3} onQuantityChange={vi.fn()} />)

    expect(screen.getByRole('img', { name: 'Mug' })).toHaveAttribute('src', '/remoteapi/catalog/items/7/pic')
    expect(screen.getByText('Mug')).toBeInTheDocument()
    expect(screen.getByText('$5.00')).toBeInTheDocument()
    expect(screen.getByLabelText('Quantity for Mug')).toHaveValue(3)
    expect(screen.getByText('$15.00')).toBeInTheDocument()
  })

  it('calls onQuantityChange with the new value', () => {
    const onQuantityChange = vi.fn()
    render(<CartLineItem item={item} quantity={1} onQuantityChange={onQuantityChange} />)

    fireEvent.change(screen.getByLabelText('Quantity for Mug'), { target: { value: '5' } })

    expect(onQuantityChange).toHaveBeenCalledExactlyOnceWith(5)
  })
})
