import { apiGet, apiPost } from '../lib/apiFetch'

export interface GrantViewModel {
  clientId: string
  clientName: string
  clientUrl: string | null
  clientLogoUrl: string | null
  description: string | null
  created: string
  expires: string | null
  identityGrantNames: string[]
  apiGrantNames: string[]
}

export interface GrantsViewModel {
  grants: GrantViewModel[]
}

export function getGrants(): Promise<GrantsViewModel> {
  return apiGet<GrantsViewModel>('/Grants/Index')
}

// clientId binds from the query string, not a JSON body -- GrantsController.Revoke
// takes a plain string parameter, and [ApiController] only infers [FromBody] for
// complex types, same reasoning already applied to Logout's logoutId.
export function revokeGrant(clientId: string): Promise<GrantsViewModel> {
  return apiPost<GrantsViewModel>(`/Grants/Revoke?clientId=${encodeURIComponent(clientId)}`)
}
