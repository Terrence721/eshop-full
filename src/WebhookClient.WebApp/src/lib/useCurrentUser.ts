import { getCurrentUser, type UserClaim } from '../api/bff'
import { useAsync } from './useAsync'

interface CurrentUserState {
  claims: UserClaim[] | null
  loading: boolean
}

// Mirrors Blazor's cascading AuthenticationState / <AuthorizeView> -- each
// component that needs to know whether the visitor is signed in calls this
// itself rather than sharing one fetch through a context provider, same
// scoped-fetch style as the rest of this app's pages. claims is null both
// while loading and when signed out; callers use `loading` to tell them
// apart.
export function useCurrentUser(): CurrentUserState {
  const { data: claims, loading } = useAsync(getCurrentUser, [])
  return { claims, loading }
}
