import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { getLoggedOut, type LoggedOutViewModel } from '../../api/account'

function LoggedOutPage() {
  const [searchParams] = useSearchParams()
  const logoutId = searchParams.get('logoutId')

  const [vm, setVm] = useState<LoggedOutViewModel | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<Error | null>(null)

  useEffect(() => {
    getLoggedOut(logoutId)
      .then(setVm)
      .catch(setLoadError)
      .finally(() => setLoading(false))
  }, [logoutId])

  useEffect(() => {
    if (vm?.automaticRedirectAfterSignOut && vm.postLogoutRedirectUri) {
      window.location.href = vm.postLogoutRedirectUri
    }
  }, [vm])

  if (loading) {
    return <p>Loading...</p>
  }

  if (loadError) {
    return <p>Could not load the logged-out page: {loadError.message}</p>
  }

  if (!vm) {
    return null
  }

  return (
    <div>
      <h1>Logged out</h1>
      <p>
        {vm.clientName
          ? `You have successfully logged out of ${vm.clientName}.`
          : 'You have successfully logged out.'}
      </p>
      {vm.postLogoutRedirectUri && !vm.automaticRedirectAfterSignOut && (
        <p>
          <a href={vm.postLogoutRedirectUri}>Click here to return to the application</a>
        </p>
      )}
      {vm.signOutIframeUrl && <iframe title="Federated sign-out" src={vm.signOutIframeUrl} hidden />}
    </div>
  )
}

export default LoggedOutPage
