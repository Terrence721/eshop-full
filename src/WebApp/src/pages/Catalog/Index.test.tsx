import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import CatalogIndex from './Index'

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <CatalogIndex />
    </MemoryRouter>,
  )
}

describe('CatalogIndex', () => {
  it('renders only the homepage image', () => {
    renderAt('/')

    const image = screen.getByRole('img', { name: 'eShop homepage' })
    expect(image).toHaveAttribute('src', '/eshop_homepage.png')
  })
})