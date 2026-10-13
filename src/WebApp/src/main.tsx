import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import './index.css'
import { MainLayout } from './components/MainLayout.tsx'
import { RequireAuth } from './components/RequireAuth.tsx'
import CartIndex from './pages/Cart/Index.tsx'
import CatalogIndex from './pages/Catalog/Index.tsx'
import ItemIndex from './pages/Item/Index.tsx'

const router = createBrowserRouter([
  {
    Component: MainLayout,
    children: [
      { path: '/', Component: CatalogIndex },
      { path: '/item/:itemId', Component: ItemIndex },
      {
        Component: RequireAuth,
        children: [{ path: '/cart', Component: CartIndex }],
      },
    ],
  },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
