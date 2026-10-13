import { Link, useSearchParams } from 'react-router'

// Mirrors Pages/Catalog/Catalog.razor's page-number links, controlled by
// the ?page= query param (1-based, matching upstream's own
// [SupplyParameterFromQuery] page).
export function Pagination({ totalPages }: { totalPages: number }) {
  const [searchParams] = useSearchParams()
  const currentPage = Number(searchParams.get('page') ?? '1')

  if (totalPages <= 1) {
    return null
  }

  function hrefForPage(page: number) {
    const params = new URLSearchParams(searchParams)
    params.set('page', String(page))
    return `?${params}`
  }

  return (
    <nav aria-label="Pagination">
      {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
        <Link key={page} to={hrefForPage(page)} aria-current={page === currentPage ? 'page' : undefined}>
          {page}
        </Link>
      ))}
    </nav>
  )
}
