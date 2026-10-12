import { render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { ReceivedMessages } from './ReceivedMessages'

// jsdom doesn't implement EventSource -- same stub useReceivedMessages.test.ts
// uses, needed here since this component renders that hook internally.
class FakeEventSource {
  static instances: FakeEventSource[] = []
  url: string

  constructor(url: string) {
    this.url = url
    FakeEventSource.instances.push(this)
  }

  addEventListener() {}

  close() {}
}

afterEach(() => {
  FakeEventSource.instances = []
})

describe('ReceivedMessages', () => {
  it('shows a loading state before the fetch resolves', () => {
    mockFetchOnce(200, [])
    vi.stubGlobal('EventSource', FakeEventSource)

    render(<ReceivedMessages />)

    expect(screen.getByText('Loading...')).toBeInTheDocument()
  })

  it('shows the empty-state message when none received', async () => {
    mockFetchOnce(200, [])
    vi.stubGlobal('EventSource', FakeEventSource)

    render(<ReceivedMessages />)

    expect(await screen.findByText('None yet received')).toBeInTheDocument()
  })

  it('renders a row per received message', async () => {
    mockFetchOnce(200, [{ when: '2026-01-01T00:00:00Z', data: '{"orderId":1}', token: 'abc' }])
    vi.stubGlobal('EventSource', FakeEventSource)

    render(<ReceivedMessages />)

    expect(await screen.findByText('{"orderId":1}')).toBeInTheDocument()
    expect(screen.getByText('abc')).toBeInTheDocument()
  })
})
