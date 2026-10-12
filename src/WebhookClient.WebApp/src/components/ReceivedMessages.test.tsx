import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { stubEventSource } from '../test/mockEventSource'
import { ReceivedMessages } from './ReceivedMessages'

describe('ReceivedMessages', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchOnce(200, [])
    stubEventSource()

    render(<ReceivedMessages />)

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('shows the empty-state message when none received', async () => {
    mockFetchOnce(200, [])
    stubEventSource()

    render(<ReceivedMessages />)

    expect(await screen.findByText('None yet received')).toBeInTheDocument()
  })

  it('renders a row per received message', async () => {
    mockFetchOnce(200, [{ when: '2026-01-01T00:00:00Z', data: '{"orderId":1}', token: 'abc' }])
    stubEventSource()

    render(<ReceivedMessages />)

    expect(await screen.findByText('{"orderId":1}')).toBeInTheDocument()
    expect(screen.getByText('abc')).toBeInTheDocument()
  })
})
