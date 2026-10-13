import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce, mockFetchSequence } from '../test/mockFetch'
import { FakeEventSource, stubEventSource } from '../test/mockEventSource'
import { useOrders } from './useOrders'

describe('useOrders', () => {
  it('fetches the initial order list on mount', async () => {
    mockFetchOnce(200, [{ orderNumber: 1, date: '2026-01-01', status: 'Submitted', total: 10 }])
    stubEventSource()

    const { result } = renderHook(() => useOrders())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.data).toEqual([{ orderNumber: 1, date: '2026-01-01', status: 'Submitted', total: 10 }])
  })

  it('opens an EventSource to the orders stream endpoint', () => {
    mockFetchOnce(200, [])
    stubEventSource()

    renderHook(() => useOrders())

    expect(FakeEventSource.instances).toHaveLength(1)
    expect(FakeEventSource.instances[0].url).toBe('/api/orders/stream')
  })

  it('refetches the list when an order-status-changed event arrives', async () => {
    mockFetchSequence({ status: 200, body: [] }, { status: 200, body: [{ orderNumber: 2, date: '2026-01-02', status: 'Paid', total: 20 }] })
    stubEventSource()

    const { result } = renderHook(() => useOrders())
    await waitFor(() => expect(result.current.loading).toBe(false))

    act(() => FakeEventSource.instances[0].dispatch('order-status-changed'))

    await waitFor(() => expect(result.current.data).toEqual([{ orderNumber: 2, date: '2026-01-02', status: 'Paid', total: 20 }]))
  })

  it('closes the EventSource on unmount', () => {
    mockFetchOnce(200, [])
    stubEventSource()

    const { unmount } = renderHook(() => useOrders())
    const instance = FakeEventSource.instances[0]

    unmount()

    expect(instance.closed).toBe(true)
  })
})
