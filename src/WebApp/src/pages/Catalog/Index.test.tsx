import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchByUrl } from '../../test/mockFetch'
import CatalogIndex from './Index'

function stubFiltersFetches() {
  return {
    '/remoteapi/catalog/catalogbrands': { status: 200, body: [] },
    '/remoteapi/catalog/catalogtypes': { status: 200, body: [] },
  }
}

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <CatalogIndex />
    </MemoryRouter>,
  )
}

describe('CatalogIndex', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchByUrl({ ...stubFiltersFetches(), '/remoteapi/catalog/items?pageIndex=0&pageSize=9': { status: 200, body: { pageIndex: 0, pageSize: 9, count: 0, data: [] } } })

    renderAt('/')

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('renders a card for every item on the page', async () => {
    const item = { id: 1, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 }
    mockFetchByUrl({
      ...stubFiltersFetches(),
      '/remoteapi/catalog/items?pageIndex=0&pageSize=9': { status: 200, body: { pageIndex: 0, pageSize: 9, count: 1, data: [item] } },
    })

    renderAt('/')

    expect(await screen.findByText('Mug')).toBeInTheDocument()
  })

  it('converts the 1-based ?page= query param to a 0-based pageIndex', async () => {
    const fetchMock = mockFetchByUrl({
      ...stubFiltersFetches(),
      '/remoteapi/catalog/items?pageIndex=2&pageSize=9': { status: 200, body: { pageIndex: 2, pageSize: 9, count: 0, data: [] } },
    })

    renderAt('/?page=3')

    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith('/remoteapi/catalog/items?pageIndex=2&pageSize=9', { headers: { 'X-CSRF': '1' } }),
    )
  })

  it('shows an error message when the fetch fails', async () => {
    mockFetchByUrl({ ...stubFiltersFetches(), '/remoteapi/catalog/items?pageIndex=0&pageSize=9': { status: 500 } })

    renderAt('/')

    expect(await screen.findByText(/failed: 500/)).toBeInTheDocument()
  })
})
