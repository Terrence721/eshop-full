import { useEffect, useState } from 'react'
import { getOrders, type OrderSummary } from '../api/orders'
import { useAsync } from './useAsync'

interface OrdersState {
  data: OrderSummary[] | null
  loading: boolean
  error: Error | null
}

// Mirrors upstream's OrdersRefreshOnStatusChange.razor: it re-fetches the
// whole order list on receipt rather than consuming any pushed payload
// (WebBFF's /api/orders/stream carries none either -- see
// OrderStatusEndpoints.cs), same "re-fetch on signal" shape
// WebhookClient.WebApp's useReceivedMessages already uses for its own
// SSE endpoint.
export function useOrders(): OrdersState {
  const [refetchTrigger, setRefetchTrigger] = useState(0)
  const { data, loading, error } = useAsync(getOrders, [refetchTrigger])

  useEffect(() => {
    const source = new EventSource('/api/orders/stream')
    const onChanged = () => setRefetchTrigger((n) => n + 1)
    source.addEventListener('order-status-changed', onChanged)
    return () => source.close()
  }, [])

  return { data, loading, error }
}
