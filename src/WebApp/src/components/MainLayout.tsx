import { Link, Outlet } from 'react-router'
import { ChatSidebar } from './ChatSidebar'

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

      <main style={{ display: 'flex', height: '100vh', gap: '1rem', padding: '1rem' }}>
        <div style={{ flex: 1 }}>
          <Outlet />
        </div>
        <ChatSidebar />
      </main>
    </>
  )
}
