import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchByUrl } from '../test/mockFetch'
import AddWebhook from './AddWebhook'

function renderPage() {
  const router = createMemoryRouter(
    [
      { path: '/add-webhook', Component: AddWebhook },
      { path: '/', element: <div>Home page</div> },
    ],
    { initialEntries: ['/add-webhook'] },
  )
  return render(<RouterProvider router={router} />)
}

describe('AddWebhook', () => {
  it('pre-fills the token field from the configured default', async () => {
    mockFetchByUrl({ '/api/hooks/default-token': { status: 200, body: { token: 'default-token-value' } } })

    renderPage()

    // Testing Library's default findBy* timeout (1000ms) is tight enough that this flakes
    // under v8 coverage instrumentation's added overhead, even though the fetch-then-render
    // it's waiting on is otherwise fast and reliable.
    expect(await screen.findByLabelText('Token:', {}, { timeout: 3000 })).toHaveValue('default-token-value')
  })

  it('links to home', async () => {
    mockFetchByUrl({ '/api/hooks/default-token': { status: 200, body: { token: '' } } })

    renderPage()

    expect(screen.getByRole('link', { name: 'home' })).toHaveAttribute('href', '/')
  })

  it('registers the token and navigates home on success', async () => {
    const fetchMock = mockFetchByUrl({
      '/api/hooks/default-token': { status: 200, body: { token: 'my-token' } },
      '/api/hooks': { status: 200 },
    })
    const user = userEvent.setup()

    renderPage()

    await user.click(await screen.findByRole('button', { name: 'Register' }))

    expect(await screen.findByText('Home page')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith('/api/hooks', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token: 'my-token' }),
    })
  })

  it('shows the rejection message and stays on the page when registration fails', async () => {
    mockFetchByUrl({
      '/api/hooks/default-token': { status: 200, body: { token: 'my-token' } },
      '/api/hooks': { status: 400 },
    })
    const user = userEvent.setup()

    renderPage()

    await user.click(await screen.findByRole('button', { name: 'Register' }))

    expect(await screen.findByText('Registation was rejected with status 400')).toBeInTheDocument()
    expect(screen.queryByText('Home page')).not.toBeInTheDocument()
  })
})
