import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import './index.css'
import { MainLayout } from './components/MainLayout.tsx'
import { RequireAuth } from './components/RequireAuth.tsx'
import CartIndex from './pages/Cart/Index.tsx'
import CatalogIndex from './pages/Catalog/Index.tsx'
import CatalogList from './pages/Catalog/List.tsx'
import CheckoutIndex from './pages/Checkout/Index.tsx'
import ItemIndex from './pages/Item/Index.tsx'
import OrdersIndex from './pages/Orders/Index.tsx'
import UserLogin from './pages/User/Login.tsx'
import UserLogout from './pages/User/Logout.tsx'

const router = createBrowserRouter([
  {
    Component: MainLayout,
    children: [
      { path: '/', Component: CatalogIndex },
      { path: '/catalog', Component: CatalogList },
      { path: '/item/:itemId', Component: ItemIndex },
      { path: '/user/login', Component: UserLogin },
      { path: '/user/logout', Component: UserLogout },
      {
        Component: RequireAuth,
        children: [
          { path: '/cart', Component: CartIndex },
          { path: '/checkout', Component: CheckoutIndex },
          { path: '/user/orders', Component: OrdersIndex },
        ],
      },
    ],
  },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
