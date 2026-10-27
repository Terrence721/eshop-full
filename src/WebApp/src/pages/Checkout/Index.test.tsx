import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { mockFetchByUrl } from '../../test/mockFetch'
import CheckoutIndex from './Index'

const claims = [
  { type: 'sub', value: 'alice-id' },
  { type: 'name', value: 'Alice' },
  { type: 'address_street', value: '1 Microsoft Way' },
  { type: 'address_city', value: 'Redmond' },
  { type: 'address_state', value: 'WA' },
  { type: 'address_country', value: 'USA' },
  { type: 'address_zip_code', value: '98052' },
]

const mug = { id: 1, name: 'Mug', description: null, price: 5, catalogTypeId: 1, catalogBrandId: 1 }

function stubs(overrides: Record<string, { status: number; body?: unknown }> = {}) {
  return {
    '/bff/user': { status: 200, body: claims },
    '/api/basket': { status: 200, body: [{ productId: 1, quantity: 2 }] },
    '/remoteapi/catalog/api/catalog/items/1': { status: 200, body: mug },
    ...overrides,
  }
}

function renderWithRoutes() {
  const router = createMemoryRouter(
    [
      { path: '/checkout', Component: CheckoutIndex },
      { path: '/user/orders', element: <div>Orders</div> },
    ],
    { initialEntries: ['/checkout'] },
  )
  return render(<RouterProvider router={router} />)
}

describe('CheckoutIndex', () => {
  it('shows an empty-bag message when the basket has no lines', async () => {
    mockFetchByUrl(stubs({ '/api/basket': { status: 200, body: [] } }))

    renderWithRoutes()

    expect(await screen.findByText('Your shopping bag is empty.')).toBeInTheDocument()
  })

  it('pre-fills the address form from the real OIDC address claims', async () => {
    mockFetchByUrl(stubs())

    renderWithRoutes()
    await screen.findByLabelText('Street')

    await waitFor(() => expect(screen.getByLabelText('Street')).toHaveValue('1 Microsoft Way'))
    expect(screen.getByLabelText('City')).toHaveValue('Redmond')
    expect(screen.getByLabelText('State')).toHaveValue('WA')
    expect(screen.getByLabelText('Country')).toHaveValue('USA')
    expect(screen.getByLabelText('Zip code')).toHaveValue('98052')
  })

  it('submits the order and navigates to /user/orders on success', async () => {
    const fetchMock = mockFetchByUrl(stubs())
    const user = userEvent.setup()

    renderWithRoutes()
    await screen.findByLabelText('Street')
    await waitFor(() => expect(screen.getByLabelText('Street')).toHaveValue('1 Microsoft Way'))

    await user.type(screen.getByLabelText('Street'), '!')
    await user.type(screen.getByLabelText('City'), '!')
    await user.type(screen.getByLabelText('State'), '!')
    await user.type(screen.getByLabelText('Country'), '!')
    await user.type(screen.getByLabelText('Zip code'), '!')
    fetchMock.mockResolvedValueOnce({ ok: true, status: 200, json: vi.fn() }) // createOrder
    fetchMock.mockResolvedValueOnce({ ok: true, status: 204, json: vi.fn() }) // clearBasket
    await user.click(screen.getByRole('button', { name: 'Place order' }))

    expect(await screen.findByText('Orders')).toBeInTheDocument()
  })

  it('shows an error message when order creation fails', async () => {
    const fetchMock = mockFetchByUrl(stubs())
    const user = userEvent.setup()

    renderWithRoutes()
    await screen.findByLabelText('Street')
    await waitFor(() => expect(screen.getByLabelText('Street')).toHaveValue('1 Microsoft Way'))

    fetchMock.mockResolvedValueOnce({ ok: false, status: 400, json: vi.fn() }) // createOrder fails
    await user.click(screen.getByRole('button', { name: 'Place order' }))

    expect(await screen.findByText(/failed: 400/)).toBeInTheDocument()
  })
})
