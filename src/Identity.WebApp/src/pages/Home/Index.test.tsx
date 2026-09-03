import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../../test/mockFetch'
import HomeIndex from './Index'

describe('HomeIndex', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchOnce(200, { version: '8.0.6', wellKnownConfigurationUrl: '', diagnosticsUrl: '', grantsUrl: '' })

    render(<HomeIndex />)

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('renders the version and links to the discovery/diagnostics/grants URLs on success', async () => {
    mockFetchOnce(200, {
      version: '8.0.6',
      wellKnownConfigurationUrl: '/.well-known/openid-configuration',
      diagnosticsUrl: '/Diagnostics/Index',
      grantsUrl: '/Grants/Index',
    })

    render(<HomeIndex />)

    expect(await screen.findByText('Duende IdentityServer version 8.0.6')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'OpenID Connect discovery document' })).toHaveAttribute('href', '/.well-known/openid-configuration')
    expect(screen.getByRole('link', { name: 'Diagnostics' })).toHaveAttribute('href', '/Diagnostics/Index')
    expect(screen.getByRole('link', { name: 'Grants' })).toHaveAttribute('href', '/Grants/Index')
  })

  it('shows the real "only available in Development" message for the non-error 404 case', async () => {
    mockFetchOnce(404)

    render(<HomeIndex />)

    expect(await screen.findByText('This page is only available in Development.')).toBeInTheDocument()
  })

  it('shows the error message when the fetch fails', async () => {
    mockFetchOnce(500)

    render(<HomeIndex />)

    expect(await screen.findByText('Could not load this page: GET /Home/Index failed: 500')).toBeInTheDocument()
  })
})
