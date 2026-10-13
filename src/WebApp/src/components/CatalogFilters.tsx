import { useNavigate, useSearchParams } from 'react-router'
import { getBrands, getTypes } from '../api/catalog'
import { useAsync } from '../lib/useAsync'

// Mirrors Pages/Catalog/CatalogSearch.razor's brand/type filter dropdowns.
// Changing a filter resets the page back to 1, matching upstream's own
// navigation behavior (a new filter invalidates the current page number).
export function CatalogFilters() {
  const { data: brands } = useAsync(getBrands, [])
  const { data: types } = useAsync(getTypes, [])
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()

  function navigateWithFilter(key: 'brand' | 'type', value: string) {
    const params = new URLSearchParams(searchParams)
    if (value) {
      params.set(key, value)
    } else {
      params.delete(key)
    }
    params.delete('page')
    navigate(`?${params}`)
  }

  return (
    <div className="catalog-filters">
      <label>
        Brand
        <select value={searchParams.get('brand') ?? ''} onChange={(e) => navigateWithFilter('brand', e.target.value)}>
          <option value="">All</option>
          {brands?.map((brand) => (
            <option key={brand.id} value={brand.id}>
              {brand.brand}
            </option>
          ))}
        </select>
      </label>
      <label>
        Type
        <select value={searchParams.get('type') ?? ''} onChange={(e) => navigateWithFilter('type', e.target.value)}>
          <option value="">All</option>
          {types?.map((type) => (
            <option key={type.id} value={type.id}>
              {type.type}
            </option>
          ))}
        </select>
      </label>
    </div>
  )
}
