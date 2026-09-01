import { useEffect, useState, type SubmitEvent } from 'react'
import { useSearchParams } from 'react-router'
import { getConsent, postConsent, type ConsentViewModel, type ScopeViewModel } from '../../api/consent'

function scopeCheckedMap(vm: ConsentViewModel): Record<string, boolean> {
  const map: Record<string, boolean> = {}
  for (const scope of [...vm.identityScopes, ...vm.apiScopes]) {
    map[scope.value] = scope.checked
  }
  return map
}

function ScopeCheckbox({
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

function ConsentPage() {
  const [searchParams] = useSearchParams()
  const returnUrl = searchParams.get('returnUrl') ?? ''

  const [vm, setVm] = useState<ConsentViewModel | null>(null)
  const [checkedScopes, setCheckedScopes] = useState<Record<string, boolean>>({})
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<Error | null>(null)
  const [notFound, setNotFound] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [validationError, setValidationError] = useState<string | null>(null)

  useEffect(() => {
    getConsent(returnUrl)
      .then((result) => {
        if (result === null) {
          setNotFound(true)
          return
        }
        setVm(result)
        setCheckedScopes(scopeCheckedMap(result))
      })
      .catch(setLoadError)
      .finally(() => setLoading(false))
  }, [returnUrl])

  if (loading) {
    return <p>Loading...</p>
  }

  if (loadError) {
    return <p>Could not load the consent page: {loadError.message}</p>
  }

  if (notFound || !vm) {
    return <p>No matching authorization request was found. It may have expired -- please try again.</p>
  }

  async function submit(button: 'yes' | 'no') {
    setSubmitting(true)
    setValidationError(null)
    try {
      const result = await postConsent({
        button,
        scopesConsented: Object.entries(checkedScopes)
          .filter(([, checked]) => checked)
          .map(([value]) => value),
        rememberConsent: vm!.rememberConsent,
        returnUrl,
        description: vm!.description,
      })
      if (result === null) {
        setNotFound(true)
        return
      }
      if (result.validationError) {
        setValidationError(result.validationError)
        if (result.viewModel) {
          setVm(result.viewModel)
          setCheckedScopes(scopeCheckedMap(result.viewModel))
        }
        return
      }
      if (result.redirectUrl) {
        // A real navigation: this completes the OIDC authorization code
        // flow back to whatever client requested it. IsNativeClient
        // handling deliberately deferred, same reasoning as Login.tsx --
        // no native client exists in this repo yet.
        window.location.href = result.redirectUrl
      }
    } catch (error) {
      setValidationError(error instanceof Error ? error.message : 'Consent failed.')
    } finally {
      setSubmitting(false)
    }
  }

  function handleSubmit(event: SubmitEvent) {
    event.preventDefault()
    void submit('yes')
  }

  return (
    <div>
      <h1>{vm.clientUrl ? <a href={vm.clientUrl}>{vm.clientName}</a> : vm.clientName}</h1>
      <p>{vm.clientName} is requesting access to the following:</p>
      {validationError && <p role="alert">{validationError}</p>}

      <form onSubmit={handleSubmit}>
        {vm.identityScopes.length > 0 && (
          <fieldset>
            <legend>Identity</legend>
            <ul>
              {vm.identityScopes.map((scope) => (
                <ScopeCheckbox
                  key={scope.value}
                  scope={scope}
                  checked={checkedScopes[scope.value] ?? false}
                  onChange={(value, checked) => setCheckedScopes((prev) => ({ ...prev, [value]: checked }))}
                />
              ))}
            </ul>
          </fieldset>
        )}

        {vm.apiScopes.length > 0 && (
          <fieldset>
            <legend>Application access</legend>
            <ul>
              {vm.apiScopes.map((scope) => (
                <ScopeCheckbox
                  key={scope.value}
                  scope={scope}
                  checked={checkedScopes[scope.value] ?? false}
                  onChange={(value, checked) => setCheckedScopes((prev) => ({ ...prev, [value]: checked }))}
                />
              ))}
            </ul>
          </fieldset>
        )}

        {vm.allowRememberConsent && (
          <div>
            <label htmlFor="rememberConsent">
              <input
                id="rememberConsent"
                type="checkbox"
                checked={vm.rememberConsent}
                onChange={(event) => setVm({ ...vm, rememberConsent: event.target.checked })}
              />
              Remember my decision
            </label>
          </div>
        )}

        <button type="submit" disabled={submitting}>
          Yes, Allow
        </button>
        <button type="button" disabled={submitting} onClick={() => void submit('no')}>
          No, Do Not Allow
        </button>
      </form>
    </div>
  )
}

export default ConsentPage
