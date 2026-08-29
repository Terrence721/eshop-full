import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router'
import './index.css'
import HomeIndex from './pages/Home/Index.tsx'
import HomeError from './pages/Home/Error.tsx'

// Route paths mirror Identity.API's own controller/action casing (e.g.
// /Home/Index, not /home/index) since Duende's redirect targets already
// use that casing -- this SPA only ever gets navigated to by Duende itself.
const router = createBrowserRouter([
  { path: '/Home/Index', Component: HomeIndex },
  { path: '/Home/Error', Component: HomeError },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
