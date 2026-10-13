import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import { Pagination } from './Pagination'

function renderAt(path: string, totalPages: number) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Pagination totalPages={totalPages} />
    </MemoryRouter>,
  )
}

describe('Pagination', () => {
  it('renders nothing when there is only one page', () => {
    const { container } = renderAt('/', 1)

    expect(container).toBeEmptyDOMElement()
  })

  it('renders a link for every page', () => {
    renderAt('/', 3)

    expect(screen.getAllByRole('link')).toHaveLength(3)
  })

  it('marks the current page from the ?page= query param', () => {
    renderAt('/?page=2', 3)

    expect(screen.getByRole('link', { name: '2' })).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('link', { name: '1' })).not.toHaveAttribute('aria-current')
  })

  it('preserves other query params while changing the page', () => {
    renderAt('/?brand=1', 2)

    expect(screen.getByRole('link', { name: '2' })).toHaveAttribute('href', '/?brand=1&page=2')
  })
})
