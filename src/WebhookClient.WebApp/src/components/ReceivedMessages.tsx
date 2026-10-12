import { useReceivedMessages } from '../lib/useReceivedMessages'

// Mirrors ReceivedMessages.razor's QuickGrid -- no interactive column
// sorting, same reasoning as RegisteredHooks (QuickGrid itself was dropped,
// no React equivalent).
export function ReceivedMessages() {
  const { data: messages, loading } = useReceivedMessages()

  if (loading) {
    return <div className="grid-placeholder">Loading...</div>
  }

  if (!messages || messages.length === 0) {
    return (
      <>
        <div className="grid-placeholder">
          <div>None yet received</div>
        </div>
        <p>Webhook messages will appear once a webhook is registered and an order transitions into "paid" status.</p>
      </>
    )
  }

  return (
    <table className="data-table">
      <thead>
        <tr>
          <th>When</th>
          <th>Data</th>
          <th>Token</th>
        </tr>
      </thead>
      <tbody>
        {messages.map((message, index) => (
          <tr key={index}>
            <td>{new Date(message.when).toLocaleString()}</td>
            <td>{message.data}</td>
            <td>{message.token}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
