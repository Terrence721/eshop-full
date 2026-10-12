import { getRegisteredHooks } from '../api/hooks'
import { useAsync } from '../lib/useAsync'

// Mirrors RegisteredHooks.razor's QuickGrid, sorted by Date descending to
// match its IsDefaultSortColumn/InitialSortDirection default view -- no
// interactive column sorting, since QuickGrid itself was dropped (no React
// equivalent) when this project split off from Blazor.
export function RegisteredHooks() {
  const { data: webhooks, loading } = useAsync(getRegisteredHooks, [])

  if (loading) {
    return <div className="grid-placeholder">Loading...</div>
  }

  if (!webhooks || webhooks.length === 0) {
    return <div className="grid-placeholder">None registered</div>
  }

  const sorted = [...webhooks].sort((a, b) => b.date.localeCompare(a.date))

  return (
    <table className="data-table">
      <thead>
        <tr>
          <th>Date</th>
          <th>Destination</th>
          <th>Token</th>
        </tr>
      </thead>
      <tbody>
        {sorted.map((hook, index) => (
          <tr key={index}>
            <td>{new Date(hook.date).toLocaleString()}</td>
            <td>{hook.destUrl}</td>
            <td>{hook.token}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
