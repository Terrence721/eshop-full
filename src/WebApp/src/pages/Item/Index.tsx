import { useParams } from 'react-router'
import { catalogItemPictureUrl, getCatalogItem } from '../../api/catalog'
import { useAsync } from '../../lib/useAsync'

// Mirrors Pages/Item/ItemPage.razor. Add-to-cart is stubbed here -- real
// wiring (useBasket, the "N in shopping bag" count, the 401-redirects-to-
// login behavior) lands with the basket vertical slice.
export default function ItemIndex() {
  const { itemId } = useParams()
  const { data: item, loading, error } = useAsync(() => getCatalogItem(Number(itemId)), [itemId])

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p className="error-message">{error.message}</p>
  }

  if (!item) {
    return <p>Sorry, we couldn't find the item you were looking for.</p>
  }

  return (
    <div className="item-page">
      <img src={catalogItemPictureUrl(item.id)} alt={item.name} />
      <div>
        <h2>{item.name}</h2>
        <p>{item.description}</p>
        <p>${item.price.toFixed(2)}</p>
        <button className="action" type="button" disabled>
          Add to shopping bag
        </button>
      </div>
    </div>
  )
}
