import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, redirect, RouterProvider } from 'react-router'
import './index.css'
import HomeIndex from './pages/Home/Index.tsx'
import HomeError from './pages/Home/Error.tsx'
import Login from './pages/Account/Login.tsx'
import Logout from './pages/Account/Logout.tsx'
import LoggedOut from './pages/Account/LoggedOut.tsx'
import Consent from './pages/Consent/Index.tsx'
import Diagnostics from './pages/Diagnostics/Index.tsx'
import Grants from './pages/Grants/Index.tsx'

// Route paths mirror Identity.API's own controller/action casing (e.g.
// /Home/Index, not /home/index) since Duende's redirect targets already
// use that casing -- this SPA only ever gets navigated to by Duende itself.
const router = createBrowserRouter([
  // AccountController.Login/LoginCancel redirect here (a plain "/") when
  // there's no real returnUrl -- confirmed live: without this route, that's
  // a genuine React Router 404, since nothing else in this SPA is mapped
  // to bare "/". Home/Index is this SPA's one real landing page.
  { path: '/', loader: () => redirect('/Home/Index') },
  { path: '/Home/Index', Component: HomeIndex },
  { path: '/Home/Error', Component: HomeError },
  { path: '/Account/Login', Component: Login },
  { path: '/Account/Logout', Component: Logout },
  { path: '/Account/LoggedOut', Component: LoggedOut },
  { path: '/Consent/Index', Component: Consent },
  { path: '/Diagnostics/Index', Component: Diagnostics },
  { path: '/Grants/Index', Component: Grants },
])

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)
