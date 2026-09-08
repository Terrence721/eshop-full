import { useState, type SubmitEvent } from 'react'
import { useSearchParams } from 'react-router'
import { getConsent, postConsent, type ConsentViewModel } from '../../api/consent'
import { ScopeConsentForm } from '../../components/ScopeConsentForm'
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
    <ScopeConsentForm
      clientName={vm.clientName}
      clientUrl={vm.clientUrl}
      identityScopes={vm.identityScopes}
      apiScopes={vm.apiScopes}
      checkedScopes={checkedScopes}
      onScopeChange={(value, checked) => setCheckedScopes((prev) => ({ ...prev, [value]: checked }))}
      allowRememberConsent={vm.allowRememberConsent}
      rememberConsent={vm.rememberConsent}
      onRememberChange={(checked) => setVm({ ...vm, rememberConsent: checked })}
      validationError={validationError}
      submitting={submitting}
      onSubmit={handleSubmit}
      onDeny={() => void submit('no')}
    />
  )
}

export default ConsentPage
