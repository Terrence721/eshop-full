import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import './index.css'
import { MainLayout } from './layout/MainLayout.tsx'
import AddWebhook from './pages/AddWebhook.tsx'
import HomeIndex from './pages/Home/Index.tsx'

const router = createBrowserRouter([
  {
    Component: MainLayout,
    children: [
      { path: '/', Component: HomeIndex },
      { path: '/add-webhook', Component: AddWebhook },
    ],
  },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
