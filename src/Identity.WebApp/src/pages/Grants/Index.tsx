import { useEffect, useState } from 'react'
import { getGrants, revokeGrant, type GrantViewModel, type GrantsViewModel } from '../../api/grants'

function GrantRow({ grant, onRevoke, revoking }: { grant: GrantViewModel; onRevoke: (clientId: string) => void; revoking: boolean }) {
  return (
    <li>
      <h2>{grant.clientUrl ? <a href={grant.clientUrl}>{grant.clientName}</a> : grant.clientName}</h2>
      {grant.description && <p>{grant.description}</p>}
      <p>Created: {new Date(grant.created).toLocaleString()}</p>
      {grant.expires && <p>Expires: {new Date(grant.expires).toLocaleString()}</p>}

      {grant.identityGrantNames.length > 0 && (
        <div>
          <strong>Identity grants:</strong> {grant.identityGrantNames.join(', ')}
        </div>
      )}
      {grant.apiGrantNames.length > 0 && (
        <div>
          <strong>API grants:</strong> {grant.apiGrantNames.join(', ')}
        </div>
      )}

      <button type="button" disabled={revoking} onClick={() => onRevoke(grant.clientId)}>
        Revoke
      </button>
    </li>
  )
}

function GrantsPage() {
  const [vm, setVm] = useState<GrantsViewModel | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<Error | null>(null)
  const [revokingClientId, setRevokingClientId] = useState<string | null>(null)

  useEffect(() => {
    getGrants()
      .then(setVm)
      .catch(setError)
      .finally(() => setLoading(false))
  }, [])

  async function handleRevoke(clientId: string) {
    setRevokingClientId(clientId)
    try {
      setVm(await revokeGrant(clientId))
    } catch (revokeError) {
      setError(revokeError instanceof Error ? revokeError : new Error('Revoke failed.'))
    } finally {
      setRevokingClientId(null)
    }
  }

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p>Could not load this page: {error.message}</p>
  }

  if (!vm || vm.grants.length === 0) {
    return <p>You have not given access to any applications.</p>
  }

  return (
    <div>
      <h1>Grants</h1>
      <p>Below is the list of applications you have given access to, and what they have access to.</p>
      <ul>
        {vm.grants.map((grant) => (
          <GrantRow key={grant.clientId} grant={grant} onRevoke={(clientId) => void handleRevoke(clientId)} revoking={revokingClientId === grant.clientId} />
        ))}
      </ul>
    </div>
  )
}

export default GrantsPage
