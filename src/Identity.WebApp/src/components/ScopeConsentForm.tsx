import type { SubmitEvent } from 'react'
import type { ScopeViewModel } from '../api/consent'
import { ScopeFieldsets } from './ScopeSelection'

// Shared by Consent and Device's confirm screens -- both show the same
// client header, scope fieldsets, remember-consent checkbox, and Yes/No
// buttons around a form, differing only in what onSubmit/onDeny/onRememberChange
// actually do with the decision.
export function ScopeConsentForm({
  clientName,
  clientUrl,
  identityScopes,
  apiScopes,
  checkedScopes,
  onScopeChange,
  allowRememberConsent,
  rememberConsent,
  onRememberChange,
  validationError,
  submitting,
  onSubmit,
  onDeny,
}: {
  clientName: string
  clientUrl: string | null
  identityScopes: ScopeViewModel[]
  apiScopes: ScopeViewModel[]
  checkedScopes: Record<string, boolean>
  onScopeChange: (value: string, checked: boolean) => void
  allowRememberConsent: boolean
  rememberConsent: boolean
  onRememberChange: (checked: boolean) => void
  validationError: string | null
  submitting: boolean
  onSubmit: (event: SubmitEvent) => void
  onDeny: () => void
}) {
  return (
    <div>
      <h1>{clientUrl ? <a href={clientUrl}>{clientName}</a> : clientName}</h1>
      <p>{clientName} is requesting access to the following:</p>
      {validationError && <p role="alert">{validationError}</p>}

      <form onSubmit={onSubmit}>
        <ScopeFieldsets identityScopes={identityScopes} apiScopes={apiScopes} checkedScopes={checkedScopes} onChange={onScopeChange} />

        {allowRememberConsent && (
          <div>
            <label htmlFor="rememberConsent">
              <input
                id="rememberConsent"
                type="checkbox"
                checked={rememberConsent}
                onChange={(event) => onRememberChange(event.target.checked)}
              />
              Remember my decision
            </label>
          </div>
        )}

        <button type="submit" disabled={submitting}>
          Yes, Allow
        </button>
        <button type="button" disabled={submitting} onClick={onDeny}>
          No, Do Not Allow
        </button>
      </form>
    </div>
  )
}
