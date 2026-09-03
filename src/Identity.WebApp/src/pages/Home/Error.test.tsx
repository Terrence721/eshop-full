import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { MemoryRouter } from 'react-router'
import { mockFetchOnce } from '../../test/mockFetch'
import HomeError from './Error'

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <HomeError />
    </MemoryRouter>,
  )
}

describe('HomeError', () => {
  it('shows "no error information" when there is no error payload', async () => {
    mockFetchOnce(200, { error: null })

    renderAt('/Home/Error')

    expect(await screen.findByText('No error information is available.')).toBeInTheDocument()
  })

  it('reads errorId from the query string and requests it', async () => {
    const fetchMock = mockFetchOnce(200, { error: null })

    renderAt('/Home/Error?errorId=abc123')

    await screen.findByText('No error information is available.')
    expect(fetchMock).toHaveBeenCalledWith('/Home/Error?errorId=abc123')
  })

  it('renders the real error payload as formatted JSON when present', async () => {
    mockFetchOnce(200, { error: { errorType: 'invalid_request', errorDescription: 'bad request' } })

    renderAt('/Home/Error?errorId=abc123')

    expect(await screen.findByText(/invalid_request/)).toBeInTheDocument()
  })

  it('shows a fetch-error message when the request fails', async () => {
    mockFetchOnce(500)

    renderAt('/Home/Error?errorId=abc123')

    expect(await screen.findByText('Could not load error details: GET /Home/Error failed: 500')).toBeInTheDocument()
  })
})
