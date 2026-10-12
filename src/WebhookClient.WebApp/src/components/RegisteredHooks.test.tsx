import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { RegisteredHooks } from './RegisteredHooks'

describe('RegisteredHooks', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchOnce(200, [])

    render(<RegisteredHooks />)

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('shows "None registered" when the list is empty', async () => {
    mockFetchOnce(200, [])

    render(<RegisteredHooks />)

    expect(await screen.findByText('None registered')).toBeInTheDocument()
  })

  it('renders a row per webhook, sorted by date descending', async () => {
    mockFetchOnce(200, [
      { date: '2026-01-01T00:00:00Z', destUrl: 'https://older.example/hook', token: 'first' },
      { date: '2026-02-01T00:00:00Z', destUrl: 'https://newer.example/hook', token: 'second' },
    ])

    render(<RegisteredHooks />)

    const rows = await screen.findAllByRole('row')
    expect(rows[1]).toHaveTextContent('https://newer.example/hook')
    expect(rows[2]).toHaveTextContent('https://older.example/hook')
  })
})
