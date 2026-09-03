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
