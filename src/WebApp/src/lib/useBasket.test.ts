import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { mockFetchByUrl, mockFetchOnce } from '../test/mockFetch'
import { useBasket } from './useBasket'

describe('useBasket', () => {
  it('resolves with the current basket lines', async () => {
    mockFetchOnce(200, [{ productId: 1, quantity: 2 }])

    const { result } = renderHook(() => useBasket())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.items).toEqual([{ productId: 1, quantity: 2 }])
  })

  it('addItem adds a new line with quantity 1', async () => {
    const fetchMock = mockFetchByUrl({
      '/api/basket': { status: 200, body: [] },
    })
    const { result } = renderHook(() => useBasket())
    await waitFor(() => expect(result.current.loading).toBe(false))

    fetchMock.mockResolvedValueOnce({ ok: true, status: 200, json: vi.fn().mockResolvedValue([{ productId: 7, quantity: 1 }]) })
    await act(() => result.current.addItem(7))

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', expect.objectContaining({ method: 'PUT', body: JSON.stringify([{ productId: 7, quantity: 1 }]) }))
    expect(result.current.items).toEqual([{ productId: 7, quantity: 1 }])
  })

  it('addItem increments an existing line instead of duplicating it', async () => {
    const fetchMock = mockFetchByUrl({
      '/api/basket': { status: 200, body: [{ productId: 7, quantity: 1 }] },
    })
    const { result } = renderHook(() => useBasket())
    await waitFor(() => expect(result.current.items).toEqual([{ productId: 7, quantity: 1 }]))

    fetchMock.mockResolvedValueOnce({ ok: true, status: 200, json: vi.fn().mockResolvedValue([{ productId: 7, quantity: 2 }]) })
    await act(() => result.current.addItem(7))

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', expect.objectContaining({ body: JSON.stringify([{ productId: 7, quantity: 2 }]) }))
  })

  it('setQuantity to 0 removes the line', async () => {
    const fetchMock = mockFetchByUrl({
      '/api/basket': { status: 200, body: [{ productId: 7, quantity: 1 }] },
    })
    const { result } = renderHook(() => useBasket())
    await waitFor(() => expect(result.current.items).toEqual([{ productId: 7, quantity: 1 }]))

    fetchMock.mockResolvedValueOnce({ ok: true, status: 200, json: vi.fn().mockResolvedValue([]) })
    await act(() => result.current.setQuantity(7, 0))

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', expect.objectContaining({ body: JSON.stringify([]) }))
    expect(result.current.items).toEqual([])
  })

  it('clear deletes the basket and resets local state to empty', async () => {
    const fetchMock = mockFetchByUrl({
      '/api/basket': { status: 200, body: [{ productId: 7, quantity: 1 }] },
    })
    const { result } = renderHook(() => useBasket())
    await waitFor(() => expect(result.current.items).toEqual([{ productId: 7, quantity: 1 }]))

    fetchMock.mockResolvedValueOnce({ ok: true, status: 204, json: vi.fn().mockResolvedValue(null) })
    await act(() => result.current.clear())

    expect(fetchMock).toHaveBeenCalledWith('/api/basket', expect.objectContaining({ method: 'DELETE' }))
    expect(result.current.items).toEqual([])
  })
})
