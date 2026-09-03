import { useEffect, useState, type SubmitEvent } from 'react'
import { useSearchParams } from 'react-router'
import { captureUserCode, getDeviceIndex, postDeviceCallback, type DeviceAuthorizationViewModel } from '../../api/device'
import { ScopeCheckbox } from '../../components/ScopeSelection'
import { scopeCheckedMap } from '../../lib/scopeCheckedMap'

type Step =
  | { kind: 'loading' }
  | { kind: 'needsCode'; error: string | null }
  | { kind: 'confirm'; vm: DeviceAuthorizationViewModel }
  | { kind: 'success' }
  | { kind: 'notFound' }
  | { kind: 'error'; message: string }

function DevicePage() {
  const [searchParams] = useSearchParams()
  const userCodeFromUrl = searchParams.get('userCode') ?? undefined

  const [step, setStep] = useState<Step>({ kind: 'loading' })
  const [checkedScopes, setCheckedScopes] = useState<Record<string, boolean>>({})
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    getDeviceIndex(userCodeFromUrl)
      .then((result) => {
        if (result === null) {
          setStep({ kind: 'notFound' })
          return
        }
        if (result.viewModel) {
          setStep({ kind: 'confirm', vm: result.viewModel })
          setCheckedScopes(scopeCheckedMap(result.viewModel))
          return
        }
        setStep({ kind: 'needsCode', error: null })
      })
      .catch((error: unknown) => setStep({ kind: 'error', message: error instanceof Error ? error.message : 'Could not load this page.' }))
    // Only ever runs once on mount -- userCodeFromUrl driving a re-fetch isn't
    // a real scenario (the URL doesn't change without a full navigation).
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function submitUserCode(userCode: string) {
    setSubmitting(true)
    try {
      const vm = await captureUserCode(userCode)
      if (vm === null) {
        setStep({ kind: 'needsCode', error: 'Invalid code -- please check it and try again.' })
        return
      }
      setStep({ kind: 'confirm', vm })
      setCheckedScopes(scopeCheckedMap(vm))
    } catch (error) {
      setStep({ kind: 'error', message: error instanceof Error ? error.message : 'Could not verify this code.' })
    } finally {
      setSubmitting(false)
    }
  }

  async function submitConsent(vm: DeviceAuthorizationViewModel, button: 'yes' | 'no') {
    setSubmitting(true)
    try {
      const outcome = await postDeviceCallback({
        userCode: vm.userCode,
        button,
        scopesConsented: Object.entries(checkedScopes)
          .filter(([, checked]) => checked)
          .map(([value]) => value),
        rememberConsent: vm.rememberConsent,
        description: vm.description,
      })
      if (outcome.outcome === 'success') {
        setStep({ kind: 'success' })
        return
      }
      if (outcome.outcome === 'notFound') {
        setStep({ kind: 'notFound' })
        return
      }
      // redisplay
      if (outcome.result.viewModel) {
        setStep({ kind: 'confirm', vm: outcome.result.viewModel })
        setCheckedScopes(scopeCheckedMap(outcome.result.viewModel))
      }
    } catch (error) {
      setStep({ kind: 'error', message: error instanceof Error ? error.message : 'Device authorization failed.' })
    } finally {
      setSubmitting(false)
    }
  }

  switch (step.kind) {
    case 'loading':
      return <p>Loading...</p>

    case 'error':
      return <p>Could not load this page: {step.message}</p>

    case 'notFound':
      return <p>Invalid or expired code. Please check the code and try again from your device.</p>

    case 'success':
      return <p>You have successfully authorized the device. You may close this window.</p>

    case 'needsCode':
      return (
        <div>
          <h1>Device Login</h1>
          <p>Enter the code displayed on your device.</p>
          {step.error && <p role="alert">{step.error}</p>}
          <form
            onSubmit={(event: SubmitEvent) => {
              event.preventDefault()
              const userCode = (new FormData(event.currentTarget as HTMLFormElement).get('userCode') as string) ?? ''
              void submitUserCode(userCode)
            }}
          >
            <input name="userCode" type="text" required disabled={submitting} />
            <button type="submit" disabled={submitting}>
              Submit
            </button>
          </form>
        </div>
      )

    case 'confirm': {
      const { vm } = step
      return (
        <div>
          <h1>{vm.clientUrl ? <a href={vm.clientUrl}>{vm.clientName}</a> : vm.clientName}</h1>
          <p>{vm.clientName} is requesting access to the following:</p>

          <form
            onSubmit={(event: SubmitEvent) => {
              event.preventDefault()
              void submitConsent(vm, 'yes')
            }}
          >
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
                    onChange={(event) => setStep({ kind: 'confirm', vm: { ...vm, rememberConsent: event.target.checked } })}
                  />
                  Remember my decision
                </label>
              </div>
            )}

            <button type="submit" disabled={submitting}>
              Yes, Allow
            </button>
            <button type="button" disabled={submitting} onClick={() => void submitConsent(vm, 'no')}>
              No, Do Not Allow
            </button>
          </form>
        </div>
      )
    }
  }
}

export default DevicePage
