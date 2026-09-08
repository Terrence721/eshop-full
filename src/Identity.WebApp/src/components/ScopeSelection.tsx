import type { ScopeViewModel } from '../api/consent'

// Shared by Consent and Device -- both confirmation screens show the same
// identity/API scope checkboxes, sourced from the same ConsentViewModel
// shape on the backend (DeviceAuthorizationViewModel extends it directly).
export function ScopeCheckbox({
  scope,
  checked,
  onChange,
}: {
  scope: ScopeViewModel
  checked: boolean
  onChange: (value: string, checked: boolean) => void
}) {
  return (
    <li>
      <label htmlFor={`scope-${scope.value}`}>
        <input
          id={`scope-${scope.value}`}
          type="checkbox"
          checked={checked}
          disabled={scope.required}
          onChange={(event) => onChange(scope.value, event.target.checked)}
        />
        {scope.displayName}
        {scope.required && ' (required)'}
      </label>
      {scope.description && <p>{scope.description}</p>}
    </li>
  )
}

// Also shared by Consent and Device -- both render the same two fieldsets
// (Identity / Application access) around a list of ScopeCheckbox, differing
// only in which state setter backs onChange.
export function ScopeFieldsets({
  identityScopes,
  apiScopes,
  checkedScopes,
  onChange,
}: {
  identityScopes: ScopeViewModel[]
  apiScopes: ScopeViewModel[]
  checkedScopes: Record<string, boolean>
  onChange: (value: string, checked: boolean) => void
}) {
  return (
    <>
      {identityScopes.length > 0 && (
        <fieldset>
          <legend>Identity</legend>
          <ul>
            {identityScopes.map((scope) => (
              <ScopeCheckbox key={scope.value} scope={scope} checked={checkedScopes[scope.value] ?? false} onChange={onChange} />
            ))}
          </ul>
        </fieldset>
      )}

      {apiScopes.length > 0 && (
        <fieldset>
          <legend>Application access</legend>
          <ul>
            {apiScopes.map((scope) => (
              <ScopeCheckbox key={scope.value} scope={scope} checked={checkedScopes[scope.value] ?? false} onChange={onChange} />
            ))}
          </ul>
        </fieldset>
      )}
    </>
  )
}
