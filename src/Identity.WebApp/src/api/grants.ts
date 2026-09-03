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

export async function getGrants(): Promise<GrantsViewModel> {
  const response = await fetch('/Grants/Index')
  if (!response.ok) {
    throw new Error(`GET /Grants/Index failed: ${response.status}`)
  }
  return response.json()
}

// clientId binds from the query string, not a JSON body -- GrantsController.Revoke
// takes a plain string parameter, and [ApiController] only infers [FromBody] for
// complex types, same reasoning already applied to Logout's logoutId.
export async function revokeGrant(clientId: string): Promise<GrantsViewModel> {
  const response = await fetch(`/Grants/Revoke?clientId=${encodeURIComponent(clientId)}`, {
    method: 'POST',
  })
  if (!response.ok) {
    throw new Error(`POST /Grants/Revoke failed: ${response.status}`)
  }
  return response.json()
}
