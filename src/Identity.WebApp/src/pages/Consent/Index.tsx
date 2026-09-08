import { useState, type SubmitEvent } from 'react'
import { useSearchParams } from 'react-router'
import { getConsent, postConsent, type ConsentViewModel } from '../../api/consent'
import { ScopeFieldsets } from '../../components/ScopeSelection'
import { scopeCheckedMap } from '../../lib/scopeCheckedMap'
import { useAsync } from '../../lib/useAsync'

function ConsentPage() {
  const [searchParams] = useSearchParams()
  const returnUrl = searchParams.get('returnUrl') ?? ''

  const [checkedScopes, setCheckedScopes] = useState<Record<string, boolean>>({})
  const { data: vm, loading, error: loadError, setData: setVm } = useAsync(
    () => getConsent(returnUrl),
    [returnUrl],
    (result: ConsentViewModel | null) => {
      if (result) {
        setCheckedScopes(scopeCheckedMap(result))
      }
    },
  )
  const [submitting, setSubmitting] = useState(false)
  const [validationError, setValidationError] = useState<string | null>(null)

  if (loading) {
    return <p>Loading...</p>
  }

  if (loadError) {
    return <p>Could not load the consent page: {loadError.message}</p>
  }

  if (!vm) {
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
        setVm(null)
        return
      }
      if (result.validationError) {
        setValidationError(result.validationError)
        if (result.viewModel) {
          setVm(result.viewModel)
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
        <ScopeFieldsets
          identityScopes={vm.identityScopes}
          apiScopes={vm.apiScopes}
          checkedScopes={checkedScopes}
          onChange={(value, checked) => setCheckedScopes((prev) => ({ ...prev, [value]: checked }))}
        />

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
