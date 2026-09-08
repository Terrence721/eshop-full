import { useEffect, useState, type DependencyList, type Dispatch, type SetStateAction } from 'react'

interface AsyncState<T> {
  data: T | null
  loading: boolean
  error: Error | null
  setData: Dispatch<SetStateAction<T | null>>
}

// Shared by every page that fetches once on mount (or when deps change) and
// renders a loading/error/data triad -- collapses the useState x3 +
// useEffect(fetch().then(setData).catch(setError).finally(...)) pattern each
// one used to hand-roll independently.
//
// Includes the cancellation guard Logout.tsx's own version of this pattern
// already had but the other 6 pages didn't: if deps change (or the component
// unmounts) before the fetch resolves, a stale response is dropped instead of
// overwriting state for a request that's no longer current.
//
// onSuccess runs synchronously inside the same .then() as setData, for
// callers that need to derive additional state from a fresh result (e.g.
// ConsentPage's checkedScopes) -- deliberately not a separate useEffect
// watching `data`, which would re-derive on every setData call (including
// ones a caller makes itself via the returned setData) rather than only on a
// genuinely new fetch.
export function useAsync<T>(fetcher: () => Promise<T>, deps: DependencyList, onSuccess?: (data: T) => void): AsyncState<T> {
  const [data, setData] = useState<T | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<Error | null>(null)

  // oxlint-disable-next-line react-hooks/exhaustive-deps -- deps is the caller's own dependency list, not this hook's; that's the whole point of a reusable fetch-on-deps-change hook
  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    fetcher()
      .then((result) => {
        if (!cancelled) {
          setData(result)
          onSuccess?.(result)
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setError(err instanceof Error ? err : new Error(String(err)))
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false)
        }
      })

    return () => {
      cancelled = true
    }
    // oxlint-disable-next-line react-hooks/exhaustive-deps -- deps is the caller's own dependency list, passed through by design; a reusable fetch-on-deps-change hook can't use an array literal here
  }, deps)

  return { data, loading, error, setData }
}
