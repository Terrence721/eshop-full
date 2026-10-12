import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { mockFetchOnce, mockFetchSequence } from '../test/mockFetch'
import { useReceivedMessages } from './useReceivedMessages'

// jsdom doesn't implement EventSource -- stubbed the same way mockFetch.ts
// stubs fetch, since this hook's only use of it is addEventListener/close.
class FakeEventSource {
  static instances: FakeEventSource[] = []
  private listeners = new Map<string, Set<(event: MessageEvent) => void>>()
  closed = false
  url: string

  constructor(url: string) {
    this.url = url
    FakeEventSource.instances.push(this)
  }

  addEventListener(type: string, listener: (event: MessageEvent) => void) {
    if (!this.listeners.has(type)) {
      this.listeners.set(type, new Set())
    }
    this.listeners.get(type)?.add(listener)
  }

  close() {
    this.closed = true
  }

  dispatch(type: string) {
    for (const listener of this.listeners.get(type) ?? []) {
      listener(new MessageEvent(type))
    }
  }
}

afterEach(() => {
  FakeEventSource.instances = []
})

describe('useReceivedMessages', () => {
  it('fetches the initial list on mount', async () => {
    mockFetchOnce(200, [{ when: '2026-01-01T00:00:00Z', data: null, token: 'abc' }])
    vi.stubGlobal('EventSource', FakeEventSource)

    const { result } = renderHook(() => useReceivedMessages())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.data).toEqual([{ when: '2026-01-01T00:00:00Z', data: null, token: 'abc' }])
  })

  it('opens an EventSource to the stream endpoint', () => {
    mockFetchOnce(200, [])
    vi.stubGlobal('EventSource', FakeEventSource)

    renderHook(() => useReceivedMessages())

    expect(FakeEventSource.instances).toHaveLength(1)
    expect(FakeEventSource.instances[0].url).toBe('/api/received/stream')
  })

  it('refetches the list when a hook-received event arrives', async () => {
    mockFetchSequence({ status: 200, body: [] }, { status: 200, body: [{ when: '2026-01-02T00:00:00Z', data: null, token: 'xyz' }] })
    vi.stubGlobal('EventSource', FakeEventSource)

    const { result } = renderHook(() => useReceivedMessages())
    await waitFor(() => expect(result.current.loading).toBe(false))

    act(() => FakeEventSource.instances[0].dispatch('hook-received'))

    await waitFor(() => expect(result.current.data).toEqual([{ when: '2026-01-02T00:00:00Z', data: null, token: 'xyz' }]))
  })

  it('closes the EventSource on unmount', () => {
    mockFetchOnce(200, [])
    vi.stubGlobal('EventSource', FakeEventSource)

    const { unmount } = renderHook(() => useReceivedMessages())
    const instance = FakeEventSource.instances[0]

    unmount()

    expect(instance.closed).toBe(true)
  })
})
