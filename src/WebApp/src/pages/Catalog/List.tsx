import { useSearchParams } from 'react-router'
import { getCatalogItems } from '../../api/catalog'
import { CatalogFilters } from '../../components/CatalogFilters'
import { CatalogItemCard } from '../../components/CatalogItemCard'
import { Pagination } from '../../components/Pagination'
import { useAsync } from '../../lib/useAsync'

const pageSize = 200

// Catalog listing (kept separate from the image-only landing page). Items are
// sorted by brand id so the "whole Catalog.API/pics" view is brand-ordered.
export default function CatalogList() {
  const [searchParams] = useSearchParams()
  const page = Number(searchParams.get('page') ?? '1')
  const brand = searchParams.get('brand')
  const type = searchParams.get('type')

  const { data, loading, error } = useAsync(
    () => getCatalogItems(page - 1, pageSize, brand ? Number(brand) : undefined, type ? Number(type) : undefined),
    [page, brand, type],
  )

  const sorted = data
    ? [...data.data].sort((a, b) =>
        a.catalogBrandId !== b.catalogBrandId
          ? a.catalogBrandId - b.catalogBrandId
          : a.id - b.id,
      )
    : []

  return (
    <div className="catalog-page">
      <CatalogFilters />

      {loading && <p>Loading...</p>}
      {error && <p className="error-message">{error.message}</p>}

      {data && (
        <>
          <div className="catalog-grid">
            {sorted.map((item) => (
              <CatalogItemCard key={item.id} item={item} />
            ))}
          </div>
          <Pagination totalPages={Math.ceil(data.count / pageSize)} />
        </>
      )}
    </div>
  )
}