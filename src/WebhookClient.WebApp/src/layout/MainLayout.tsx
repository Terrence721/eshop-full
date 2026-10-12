import { Link, Outlet } from 'react-router'
import { UserMenu } from '../components/UserMenu'

// Mirrors Layout/MainLayout.razor -- the title link uses react-router's
// Link, not a plain <a>, since Blazor's router intercepts same-origin
// anchor clicks by default: the real upstream behavior is a client-side
// navigation, which Link replicates (a plain <a> here would force a full
// page reload instead). The #blazor-error-ui reconnection banner has no
// SPA equivalent and is dropped, same reasoning as Program.cs's dropped
// exception-handler/HSTS block.
export function MainLayout() {
  return (
    <>
      <header>
        <h1>
          <Link to="/">Order management</Link>
        </h1>
        <div className="user-info">
          <UserMenu />
        </div>
      </header>

      <main>
        <Outlet />
      </main>
    </>
  )
}
