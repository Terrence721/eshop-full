// ExternalController has no fetch()-able surface at all: Challenge always
// returns a real ChallengeResult (a redirect to the external provider),
// and Callback is never called by frontend code directly -- it's only ever
// reached by the provider's own redirect back. The one real integration
// point is building the URL a "Login with X" button navigates to.
export function buildExternalChallengeUrl(scheme: string, returnUrl: string | null): string {
  const params = new URLSearchParams({ scheme })
  if (returnUrl) {
    params.set('returnUrl', returnUrl)
  }
  return `/External/Challenge?${params.toString()}`
}
