// Shared by RequireAuth and anywhere else that discovers mid-interaction
// that the visitor isn't signed in (e.g. a basket call that comes back
// 401). A real top-level navigation, not client-side routing -- /user/login
// decides whether to kick off the real OIDC challenge or bounce back.
export function redirectToLogin(returnUrl: string) {
  window.location.href = `/user/login?returnUrl=${encodeURIComponent(returnUrl)}`
}
