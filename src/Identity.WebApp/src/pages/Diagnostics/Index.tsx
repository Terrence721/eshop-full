import { getDiagnostics } from '../../api/diagnostics'
import { useAsync } from '../../lib/useAsync'

function DiagnosticsPage() {
  const { data: vm, loading, error } = useAsync(getDiagnostics, [])

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p>Could not load this page: {error.message}</p>
  }

  if (!vm) {
    return <p>This page is only available from localhost.</p>
  }

  return (
    <div>
      <h1>Diagnostics</h1>

      <h2>Clients</h2>
      {vm.clients.length > 0 ? (
        <ul>
          {vm.clients.map((client) => (
            <li key={client}>{client}</li>
          ))}
        </ul>
      ) : (
        <p>No clients.</p>
      )}

      <h2>Claims</h2>
      <table>
        <thead>
          <tr>
            <th>Type</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          {vm.claims.map((claim, index) => (
            <tr key={`${claim.type}-${index}`}>
              <td>{claim.type}</td>
              <td>{claim.value}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <h2>Properties</h2>
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          {Object.entries(vm.properties).map(([name, value]) => (
            <tr key={name}>
              <td>{name}</td>
              <td>{value}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default DiagnosticsPage
