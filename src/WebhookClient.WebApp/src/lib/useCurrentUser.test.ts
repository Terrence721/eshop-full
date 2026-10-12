import { renderHook, waitFor } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { useCurrentUser } from './useCurrentUser'

describe('useCurrentUser', () => {
  it('starts in a loading state with no claims', () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    const { result } = renderHook(() => useCurrentUser())

    expect(result.current.loading).toBe(true)
    expect(result.current.claims).toBeNull()
  })

  it('resolves with the claims array when signed in', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    const { result } = renderHook(() => useCurrentUser())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.claims).toEqual([{ type: 'name', value: 'alice' }])
  })

  it('resolves with null claims when signed out', async () => {
    mockFetchOnce(401)

    const { result } = renderHook(() => useCurrentUser())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.claims).toBeNull()
  })
})
