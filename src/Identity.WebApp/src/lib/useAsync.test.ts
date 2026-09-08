import { act, renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { useAsync } from './useAsync'

function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (error: unknown) => void
  const promise = new Promise<T>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}

describe('useAsync', () => {
  it('starts in a loading state with no data or error', () => {
    const { promise } = deferred<string>()
    const { result } = renderHook(() => useAsync(() => promise, []))

    expect(result.current.loading).toBe(true)
    expect(result.current.data).toBeNull()
    expect(result.current.error).toBeNull()
  })

  it('resolves with data and clears loading on success', async () => {
    const { promise, resolve } = deferred<string>()
    const { result } = renderHook(() => useAsync(() => promise, []))

    act(() => resolve('hello'))
    await waitFor(() => expect(result.current.loading).toBe(false))

    expect(result.current.data).toBe('hello')
    expect(result.current.error).toBeNull()
  })

  it('sets error and clears loading on rejection', async () => {
    const { promise, reject } = deferred<string>()
    const { result } = renderHook(() => useAsync(() => promise, []))

    act(() => reject(new Error('boom')))
    await waitFor(() => expect(result.current.loading).toBe(false))

    expect(result.current.error?.message).toBe('boom')
    expect(result.current.data).toBeNull()
  })

  it('wraps a non-Error rejection', async () => {
    const { promise, reject } = deferred<string>()
    const { result } = renderHook(() => useAsync(() => promise, []))

    act(() => reject('plain string failure'))
    await waitFor(() => expect(result.current.loading).toBe(false))

    expect(result.current.error?.message).toBe('plain string failure')
  })

  it('calls onSuccess synchronously with the resolved value', async () => {
    const { promise, resolve } = deferred<string>()
    const onSuccess = vi.fn()
    const { result } = renderHook(() => useAsync(() => promise, [], onSuccess))

    act(() => resolve('hello'))
    await waitFor(() => expect(result.current.loading).toBe(false))

    expect(onSuccess).toHaveBeenCalledExactlyOnceWith('hello')
  })

  it('exposes setData for callers that need to update the result themselves', async () => {
    const { promise, resolve } = deferred<string>()
    const { result } = renderHook(() => useAsync(() => promise, []))

    act(() => resolve('first'))
    await waitFor(() => expect(result.current.data).toBe('first'))

    act(() => result.current.setData('second'))

    expect(result.current.data).toBe('second')
  })

  it('drops a stale response when deps change before the first fetch resolves', async () => {
    const first = deferred<string>()
    const second = deferred<string>()
    const fetcher = vi.fn().mockReturnValueOnce(first.promise).mockReturnValueOnce(second.promise)

    const { result, rerender } = renderHook(({ id }: { id: number }) => useAsync(fetcher, [id]), {
      initialProps: { id: 1 },
    })

    rerender({ id: 2 })
    act(() => second.resolve('from second call'))
    await waitFor(() => expect(result.current.data).toBe('from second call'))

    // The first call's promise resolves after the second has already
    // completed -- its result must not clobber the newer one.
    act(() => first.resolve('stale, from first call'))

    expect(result.current.data).toBe('from second call')
  })
})
