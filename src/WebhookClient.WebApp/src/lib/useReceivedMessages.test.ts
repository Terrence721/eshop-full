import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce, mockFetchSequence } from '../test/mockFetch'
import { FakeEventSource, stubEventSource } from '../test/mockEventSource'
import { useReceivedMessages } from './useReceivedMessages'

describe('useReceivedMessages', () => {
  it('fetches the initial list on mount', async () => {
    mockFetchOnce(200, [{ when: '2026-01-01T00:00:00Z', data: null, token: 'abc' }])
    stubEventSource()

    const { result } = renderHook(() => useReceivedMessages())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.data).toEqual([{ when: '2026-01-01T00:00:00Z', data: null, token: 'abc' }])
  })

  it('opens an EventSource to the stream endpoint', () => {
    mockFetchOnce(200, [])
    stubEventSource()

    renderHook(() => useReceivedMessages())

    expect(FakeEventSource.instances).toHaveLength(1)
    expect(FakeEventSource.instances[0].url).toBe('/api/received/stream')
  })

  it('refetches the list when a hook-received event arrives', async () => {
    mockFetchSequence({ status: 200, body: [] }, { status: 200, body: [{ when: '2026-01-02T00:00:00Z', data: null, token: 'xyz' }] })
    stubEventSource()

    const { result } = renderHook(() => useReceivedMessages())
    await waitFor(() => expect(result.current.loading).toBe(false))

    act(() => FakeEventSource.instances[0].dispatch('hook-received'))

    await waitFor(() => expect(result.current.data).toEqual([{ when: '2026-01-02T00:00:00Z', data: null, token: 'xyz' }]))
  })

  it('closes the EventSource on unmount', () => {
    mockFetchOnce(200, [])
    stubEventSource()

    const { unmount } = renderHook(() => useReceivedMessages())
    const instance = FakeEventSource.instances[0]

    unmount()

    expect(instance.closed).toBe(true)
  })
})
