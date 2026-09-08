import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { ScopeConsentForm } from './ScopeConsentForm'

function baseProps() {
  return {
    clientName: 'WebApp Client',
    clientUrl: null as string | null,
    identityScopes: [],
    apiScopes: [],
    checkedScopes: {},
    onScopeChange: vi.fn(),
    allowRememberConsent: true,
    rememberConsent: false,
    onRememberChange: vi.fn(),
    validationError: null as string | null,
    submitting: false,
    onSubmit: vi.fn(),
    onDeny: vi.fn(),
  }
}

describe('ScopeConsentForm', () => {
  it('renders the client name as plain text when clientUrl is null', () => {
    render(<ScopeConsentForm {...baseProps()} />)

    expect(screen.getByRole('heading', { name: 'WebApp Client' })).toBeInTheDocument()
    expect(screen.queryByRole('link')).not.toBeInTheDocument()
  })

  it('renders the client name as a link when clientUrl is set', () => {
    render(<ScopeConsentForm {...baseProps()} clientUrl="https://client.example" />)

    expect(screen.getByRole('link', { name: 'WebApp Client' })).toHaveAttribute('href', 'https://client.example')
  })

  it('shows the validation error when set', () => {
    render(<ScopeConsentForm {...baseProps()} validationError="Something went wrong" />)

    expect(screen.getByRole('alert')).toHaveTextContent('Something went wrong')
  })

  it('renders no alert when validationError is null', () => {
    render(<ScopeConsentForm {...baseProps()} />)

    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })

  it('omits the remember-consent checkbox when allowRememberConsent is false', () => {
    render(<ScopeConsentForm {...baseProps()} allowRememberConsent={false} />)

    expect(screen.queryByLabelText('Remember my decision')).not.toBeInTheDocument()
  })

  it('calls onRememberChange when the remember-consent checkbox is toggled', async () => {
    const onRememberChange = vi.fn()
    const user = userEvent.setup()
    render(<ScopeConsentForm {...baseProps()} onRememberChange={onRememberChange} />)

    await user.click(screen.getByLabelText('Remember my decision'))

    expect(onRememberChange).toHaveBeenCalledWith(true)
  })

  it('disables both buttons while submitting', () => {
    render(<ScopeConsentForm {...baseProps()} submitting={true} />)

    expect(screen.getByRole('button', { name: 'Yes, Allow' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'No, Do Not Allow' })).toBeDisabled()
  })

  it('calls onDeny when the deny button is clicked', async () => {
    const onDeny = vi.fn()
    const user = userEvent.setup()
    render(<ScopeConsentForm {...baseProps()} onDeny={onDeny} />)

    await user.click(screen.getByRole('button', { name: 'No, Do Not Allow' }))

    expect(onDeny).toHaveBeenCalledTimes(1)
  })
})
