import { useOrders } from '../../lib/useOrders'

// Mirrors Pages/User/Orders.razor. List only -- upstream has no
// order-detail page/route either. Live-updates via useOrders' SSE
// subscription rather than a manual refresh button.
export default function OrdersIndex() {
  const { data: orders, loading, error } = useOrders()

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p className="error-message">{error.message}</p>
  }

  if (!orders || orders.length === 0) {
    return <p>You have no orders yet.</p>
  }

  return (
    <table className="data-table">
      <thead>
        <tr>
          <th>Order number</th>
          <th>Date</th>
          <th>Status</th>
          <th>Total</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <tr key={order.orderNumber}>
            <td>{order.orderNumber}</td>
            <td>{new Date(order.date).toLocaleDateString()}</td>
            <td>{order.status}</td>
            <td>${order.total.toFixed(2)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
