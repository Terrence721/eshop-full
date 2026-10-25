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

  it('renders a Log in avatar link with the current path as returnUrl when signed out', async () => {
    mockFetchOnce(401)

    renderAt('/item/5?foo=bar')

    const logIn = await screen.findByRole('link', { name: 'Log in' })
    expect(logIn).toHaveAttribute('href', '/user/login?returnUrl=%2Fitem%2F5%3Ffoo%3Dbar')
  })

  it('shows the user name and a Log out link to /user/logout when signed in', async () => {
    mockFetchOnce(200, [{ type: 'name', value: 'alice' }])

    renderAt('/')

    expect(await screen.findByText('alice')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Log out' })).toHaveAttribute('href', '/user/logout')
  })
})
