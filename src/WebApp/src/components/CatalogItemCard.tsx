import { Link } from 'react-router'
import { catalogItemPictureUrl, type CatalogItem } from '../api/catalog'

// Mirrors Pages/Catalog/Catalog.razor's per-item grid tile (CatalogListItem
// in upstream's WebAppComponents).
export function CatalogItemCard({ item }: { item: CatalogItem }) {
  return (
    <Link to={`/item/${item.id}`} className="catalog-item-card">
      <img src={catalogItemPictureUrl(item.id)} alt={item.name} />
      <h3>{item.name}</h3>
      <p>${item.price.toFixed(2)}</p>
    </Link>
  )
}
