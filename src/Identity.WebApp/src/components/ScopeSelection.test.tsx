import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import type { ScopeViewModel } from '../api/consent'
import { ScopeCheckbox } from './ScopeSelection'

function scope(overrides: Partial<ScopeViewModel> = {}): ScopeViewModel {
  return {
    value: 'openid',
    displayName: 'Your user identifier',
    description: null,
    emphasize: false,
    required: false,
    checked: false,
    ...overrides,
  }
}

describe('ScopeCheckbox', () => {
  it('renders the display name and links the checkbox to its label', () => {
    render(<ScopeCheckbox scope={scope()} checked={false} onChange={vi.fn()} />)

    const checkbox = screen.getByLabelText('Your user identifier')
    expect(checkbox).toHaveAttribute('id', 'scope-openid')
  })

  it('reflects the checked prop', () => {
    render(<ScopeCheckbox scope={scope({ checked: true })} checked={true} onChange={vi.fn()} />)

    expect(screen.getByRole('checkbox')).toBeChecked()
  })

  it('appends "(required)" and disables the checkbox for a required scope', () => {
    render(<ScopeCheckbox scope={scope({ required: true })} checked={true} onChange={vi.fn()} />)

    expect(screen.getByText(/\(required\)/)).toBeInTheDocument()
    expect(screen.getByRole('checkbox')).toBeDisabled()
  })

  it('does not append "(required)" or disable the checkbox for an optional scope', () => {
    render(<ScopeCheckbox scope={scope({ required: false })} checked={false} onChange={vi.fn()} />)

    expect(screen.queryByText(/\(required\)/)).not.toBeInTheDocument()
    expect(screen.getByRole('checkbox')).toBeEnabled()
  })

  it('renders the description when present', () => {
    render(<ScopeCheckbox scope={scope({ description: 'Your user profile information' })} checked={false} onChange={vi.fn()} />)

    expect(screen.getByText('Your user profile information')).toBeInTheDocument()
  })

  it('renders no description paragraph when null', () => {
    render(<ScopeCheckbox scope={scope({ description: null })} checked={false} onChange={vi.fn()} />)

    expect(screen.queryByRole('paragraph')).not.toBeInTheDocument()
  })

  it('calls onChange with the scope value and new checked state when toggled', async () => {
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<ScopeCheckbox scope={scope({ value: 'profile' })} checked={false} onChange={onChange} />)

    await user.click(screen.getByRole('checkbox'))

    expect(onChange).toHaveBeenCalledWith('profile', true)
  })

  it('does not call onChange when the checkbox is disabled (required)', async () => {
    const onChange = vi.fn()
    const user = userEvent.setup()
    render(<ScopeCheckbox scope={scope({ required: true })} checked={true} onChange={onChange} />)

    await user.click(screen.getByRole('checkbox'))

    expect(onChange).not.toHaveBeenCalled()
  })
})
