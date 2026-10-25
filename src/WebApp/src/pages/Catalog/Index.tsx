// Landing page now shows the homepage image only; the product catalog grid,
// filters, and pagination are no longer rendered here.
export default function CatalogIndex() {
  return (
    <img
      src="/eshop_homepage.png"
      alt="eShop homepage"
      className="homepage-image"
      style={{ width: '100%', height: 'auto', display: 'block' }}
    />
  )
}
