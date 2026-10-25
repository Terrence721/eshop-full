import { Link, useLocation } from 'react-router'

// Landing page now shows the homepage image only; the product catalog grid,
// filters, and pagination are no longer rendered here. The person icon baked
// into the upper-right of eshop_homepage.png acts as the login affordance.
export default function CatalogIndex() {
  const location = useLocation()
  const returnUrl = location.pathname + location.search

  return (
    <div style={{ position: 'relative', display: 'inline-block', width: '100%' }}>
      <img
        src="/eshop_homepage.png"
        alt="eShop homepage"
        className="homepage-image"
        style={{ width: '100%', height: 'auto', display: 'block' }}
      />
      <Link
        to={`/user/login?returnUrl=${encodeURIComponent(returnUrl)}`}
        aria-label="Log in"
        style={{
          position: 'absolute',
          top: '3%',
          right: '8.5%',
          width: '5%',
          height: '4%',
          display: 'block',
        }}
      />
    </div>
  )
}
