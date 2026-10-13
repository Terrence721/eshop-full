import { useParams } from 'react-router'
import { catalogItemPictureUrl, getCatalogItem } from '../../api/catalog'
import { useAsync } from '../../lib/useAsync'
import { useBasket } from '../../lib/useBasket'

// Mirrors Pages/Item/ItemPage.razor. The 401-redirects-to-login behavior
// for an unauthenticated add-to-cart lands with the auth-gating slice,
// once auth.ts exists.
export default function ItemIndex() {
  const { itemId } = useParams()
  const { data: item, loading, error } = useAsync(() => getCatalogItem(Number(itemId)), [itemId])
  const { items: basketItems, addItem } = useBasket()

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p className="error-message">{error.message}</p>
  }

  if (!item) {
    return <p>Sorry, we couldn't find the item you were looking for.</p>
  }

  const bagCount = basketItems?.reduce((total, line) => total + line.quantity, 0) ?? 0

  return (
    <div className="item-page">
      <img src={catalogItemPictureUrl(item.id)} alt={item.name} />
      <div>
        <h2>{item.name}</h2>
        <p>{item.description}</p>
        <p>${item.price.toFixed(2)}</p>
        <button className="action" type="button" onClick={() => addItem(item.id)}>
          Add to shopping bag
        </button>
        <p>{bagCount} in your shopping bag</p>
      </div>
    </div>
  )
}
