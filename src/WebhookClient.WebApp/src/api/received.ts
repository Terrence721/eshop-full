import { apiGet } from '../lib/apiFetch'

export interface WebHookReceived {
  when: string
  data: string | null
  token: string | null
}

export function getReceivedMessages(): Promise<WebHookReceived[]> {
  return apiGet<WebHookReceived[]>('/api/received')
}
