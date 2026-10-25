import { useSearchParams } from 'react-router'
import { getCatalogItems } from '../../api/catalog'
import { CatalogFilters } from '../../components/CatalogFilters'
import { CatalogItemCard } from '../../components/CatalogItemCard'
import { Pagination } from '../../components/Pagination'
import { useAsync } from '../../lib/useAsync'

const pageSize = 9

// Mirrors Pages/Catalog/Catalog.razor. page is 1-based in the URL
// (matching upstream's own [SupplyParameterFromQuery] page and
// Pagination's own convention); the API's pageIndex is 0-based.
export default function CatalogIndex() {
  const [searchParams] = useSearchParams()
  const page = Number(searchParams.get('page') ?? '1')
  const brand = searchParams.get('brand')
  const type = searchParams.get('type')

  const { data, loading, error } = useAsync(
    () => getCatalogItems(page - 1, pageSize, brand ? Number(brand) : undefined, type ? Number(type) : undefined),
    [page, brand, type],
  )

  return (
    <div className="catalog-page">
      <img
        src="/eshop_homepage.png"
        alt="eShop homepage"
        className="homepage-image"
        style={{ width: '100%', height: 'auto', display: 'block' }}
      />
      <CatalogFilters />

      {loading && <p>Loading...</p>}
      {error && <p className="error-message">{error.message}</p>}

      {data && (
        <>
          <div className="catalog-grid">
            {data.data.map((item) => (
              <CatalogItemCard key={item.id} item={item} />
            ))}
          </div>
          <Pagination totalPages={Math.ceil(data.count / pageSize)} />
        </>
      )}
    </div>
  )
}
