import { Link, Outlet } from 'react-router'

// Mirrors Layout/MainLayout.razor -- the title link uses react-router's
// Link, not a plain <a>, since Blazor's router intercepts same-origin
// anchor clicks by default: the real upstream behavior is a client-side
// navigation, which Link replicates (a plain <a> here would force a full
// page reload instead).
export function MainLayout() {
  return (
    <>
      <header>
        <h1>
          <Link to="/">eShop</Link>
        </h1>
      </header>

      <main>
        <Outlet />
      </main>
    </>
  )
}
