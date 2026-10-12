import { useEffect, useState } from 'react'
import { getReceivedMessages, type WebHookReceived } from '../api/received'
import { useAsync } from './useAsync'

interface ReceivedMessagesState {
  data: WebHookReceived[] | null
  loading: boolean
  error: Error | null
}

// Mirrors ReceivedMessages.razor: HooksRepository.Subscribe's callback just
// re-runs RefreshDataAsync() on every new message rather than appending the
// pushed item itself -- same behavior here, a refetch triggered by the
// SSE stream's "hook-received" event rather than a direct repository
// callback.
export function useReceivedMessages(): ReceivedMessagesState {
  const [refetchTrigger, setRefetchTrigger] = useState(0)
  const { data, loading, error } = useAsync(getReceivedMessages, [refetchTrigger])

  useEffect(() => {
    const source = new EventSource('/api/received/stream')
    const onReceived = () => setRefetchTrigger((n) => n + 1)
    source.addEventListener('hook-received', onReceived)
    return () => source.close()
  }, [])

  return { data, loading, error }
}
