import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router'
import { describe, expect, it, beforeEach } from 'vitest'
import CatalogIndex from './Index'

function renderAt(path: string) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <CatalogIndex />
    </MemoryRouter>,
  )
}

describe('CatalogIndex', () => {
  beforeEach(() => {
    window.sessionStorage.clear()
  })

  it('renders only the homepage image', () => {
    renderAt('/')

    const image = screen.getByRole('img', { name: 'eShop homepage' })
    expect(image).toHaveAttribute('src', '/eshop_homepage.png')
  })

  it('opens the login dialog', async () => {
    const user = userEvent.setup()
    renderAt('/')

    await user.click(screen.getByRole('button', { name: 'Log in' }))

    expect(screen.getByRole('dialog')).toBeInTheDocument()
  })

  it('shows a validation error when the username is empty', async () => {
    const user = userEvent.setup()
    renderAt('/')

    await user.click(screen.getAllByRole('button', { name: 'Log in' })[0])
    const dialog = screen.getByRole('dialog')
    await user.click(within(dialog).getByRole('button', { name: 'Log in' }))

    expect(screen.getByText('Username is required')).toBeInTheDocument()
  })

  it('shows a validation error when the password is empty', async () => {
    const user = userEvent.setup()
    renderAt('/')

    await user.click(screen.getAllByRole('button', { name: 'Log in' })[0])
    await user.type(screen.getByPlaceholderText('Username'), 'alice')
    const dialog = screen.getByRole('dialog')
    await user.click(within(dialog).getByRole('button', { name: 'Log in' }))

    expect(screen.getByText('Password is required')).toBeInTheDocument()
  })

  it('signs in and signs out', async () => {
    const user = userEvent.setup()
    renderAt('/')

    await user.click(screen.getAllByRole('button', { name: 'Log in' })[0])
    await user.type(screen.getByPlaceholderText('Username'), 'alice')
    await user.type(screen.getByPlaceholderText('Password'), 'secret')
    const dialog = screen.getByRole('dialog')
    await user.click(within(dialog).getByRole('button', { name: 'Log in' }))

    expect(screen.getByText('Login successful')).toBeInTheDocument()
    expect(screen.getByText('Welcome, alice!')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Sign out' }))

    expect(screen.queryByText('Login successful')).not.toBeInTheDocument()
  })

  it('navigates from homepage hotspots', async () => {
    const user = userEvent.setup()
    renderAt('/')

    await user.click(screen.getByRole('button', { name: 'All catalog items' }))
    await user.click(screen.getByRole('button', { name: 'AirStrider catalog items' }))
  })

  it('cancels the login dialog', async () => {
    const user = userEvent.setup()
    renderAt('/')

    await user.click(screen.getAllByRole('button', { name: 'Log in' })[0])
    const dialog = screen.getByRole('dialog')
    await user.click(within(dialog).getByRole('button', { name: 'Cancel' }))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('activates every catalog hotspot', async () => {
    const user = userEvent.setup()
    renderAt('/')

    const buttons = screen
      .getAllByRole('button')
      .filter((button) => button.getAttribute('aria-label') !== 'Log in')
    for (const button of buttons) {
      await user.click(button)
    }
  })

  it('restores an existing signed-in user from session storage', async () => {
    window.sessionStorage.setItem('demo:user', 'bob')
    const user = userEvent.setup()

    renderAt('/')

    await user.click(screen.getAllByRole('button', { name: 'Log in' })[0])

    await waitFor(() => {
      expect(screen.getByText('Welcome, bob!')).toBeInTheDocument()
    })
  })
})
