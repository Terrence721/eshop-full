import { vi } from 'vitest'

interface StubbedResponse {
  status: number
  body?: unknown
}

function toResponse({ status, body }: StubbedResponse) {
  return { ok: status >= 200 && status < 300, status, json: vi.fn().mockResolvedValue(body) }
}

// Shared across every api/*.ts test file -- each one only ever needs to
// stub a single fetch() call's status/body per test case.
export function mockFetchOnce(status: number, body?: unknown) {
  const fetchMock = vi.fn().mockResolvedValue(toResponse({ status, body }))
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

// For a page component that makes more than one distinct fetch() call in a
// single test (e.g. an initial GET on mount, then a POST on submit that
// needs a different response) -- responses are consumed in order, one per
// call, and the last one repeats for any further calls.
export function mockFetchSequence(...responses: StubbedResponse[]) {
  const fetchMock = vi.fn()
  for (const response of responses) {
    fetchMock.mockResolvedValueOnce(toResponse(response))
  }
  if (responses.length > 0) {
    fetchMock.mockResolvedValue(toResponse(responses[responses.length - 1]))
  }
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}
