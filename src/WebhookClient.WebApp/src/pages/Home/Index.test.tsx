import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchByUrl } from '../../test/mockFetch'
import { stubEventSource } from '../../test/mockEventSource'
import HomeIndex from './Index'

function renderPage() {
  return render(
    <MemoryRouter>
      <HomeIndex />
    </MemoryRouter>,
  )
}

describe('HomeIndex', () => {
  it('shows a placeholder instead of RegisteredHooks when signed out', async () => {
    mockFetchByUrl({
      '/bff/user': { status: 401 },
      '/api/received': { status: 200, body: [] },
    })
    stubEventSource()

    renderPage()

    expect(screen.getByRole('heading', { name: 'Registered webhooks' })).toBeInTheDocument()
    expect(await screen.findByText('Log in to view or edit webhook registrations')).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Add webhook registration' })).not.toBeInTheDocument()
  })

  it('shows RegisteredHooks and the add-webhook link when signed in', async () => {
    mockFetchByUrl({
      '/bff/user': { status: 200, body: [{ type: 'name', value: 'alice' }] },
      '/api/hooks': { status: 200, body: [] },
      '/api/received': { status: 200, body: [] },
    })
    stubEventSource()

    renderPage()

    const link = await screen.findByRole('link', { name: 'Add webhook registration' })
    expect(link).toHaveAttribute('href', '/add-webhook')
    expect(await screen.findByText('None registered')).toBeInTheDocument()
  })

  it('always renders the received-messages section', async () => {
    mockFetchByUrl({
      '/bff/user': { status: 401 },
      '/api/received': { status: 200, body: [] },
    })
    stubEventSource()

    renderPage()

    expect(screen.getByRole('heading', { name: 'Webhook messages received (orders paid)' })).toBeInTheDocument()
    expect(await screen.findByText('None yet received')).toBeInTheDocument()
  })
})
