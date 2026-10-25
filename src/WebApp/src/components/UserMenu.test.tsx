import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import { mockFetchOnce } from '../test/mockFetch'
import { UserMenu } from './UserMenu'

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <UserMenu />
    </MemoryRouter>,
  )
}

describe('UserMenu', () => {
  it('renders nothing while loading', () => {
    mockFetchOnce(401)

    const { container } = renderAt('/')

    expect(container).toBeEmptyDOMElement()
  })

  it('renders nothing when signed out', async () => {
    mockFetchOnce(401)

    const { container } = renderAt('/')

    await new Promise((resolve) => setTimeout(resolve, 0))
    expect(container).toBeEmptyDOMElement()
  })

  it('shows the user name and a Log out link to /user/logout when signed in', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    renderAt('/')

    expect(await screen.findByText('alice')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Log out' })).toHaveAttribute('href', '/user/logout')
  })
})
