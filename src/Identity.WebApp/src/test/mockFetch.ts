import { vi } from 'vitest'

// Shared across every api/*.ts test file -- each one only ever needs to
// stub a single fetch() call's status/body per test case.
export function mockFetchOnce(status: number, body?: unknown) {
  const json = vi.fn().mockResolvedValue(body)
  const fetchMock = vi.fn().mockResolvedValue({ ok: status >= 200 && status < 300, status, json })
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}
