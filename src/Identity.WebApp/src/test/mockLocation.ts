// Shared by every page that does a real top-level navigation via
// window.location.href = ... (Login, Logout, LoggedOut, Consent, Device) --
// jsdom's real Location object doesn't support being navigated, so this
// swaps it for a plain settable object, letting a test assert on the exact
// URL the component tried to send the browser to.
export function mockWindowLocation() {
  const original = window.location
  const location = { href: '' }
  // @ts-expect-error -- deliberately replacing the readonly-typed global for the test
  delete window.location
  // @ts-expect-error -- same as above
  window.location = location

  return {
    location,
    restore: () => {
      // @ts-expect-error -- restoring the readonly-typed global after the test
      window.location = original
    },
  }
}
