import { Link } from 'react-router'
import { getCatalogItem, type CatalogItem } from '../../api/catalog'
import { CartLineItem } from '../../components/CartLineItem'
import { useAsync } from '../../lib/useAsync'
import { useBasket } from '../../lib/useBasket'

// Mirrors Pages/Cart/CartPage.razor. Catalog.API has no batch-by-id
// endpoint on its real REST contract (only GET /items/{id} singular), so
// basket lines are resolved to catalog details via one call per line --
// a deliberate, flagged choice, not a missing feature.
export default function CartIndex() {
  const { items: basketItems, loading: basketLoading, error: basketError, setQuantity } = useBasket()

  const { data: catalogItems, loading: catalogLoading, error: catalogError } = useAsync(
    async () => {
      if (!basketItems || basketItems.length === 0) {
        return [] as CatalogItem[]
      }
      return Promise.all(basketItems.map((line) => getCatalogItem(line.productId)))
    },
    [basketItems],
  )

  if (basketLoading || catalogLoading) {
    return <p>Loading...</p>
  }

  if (basketError || catalogError) {
    return <p className="error-message">{(basketError ?? catalogError)?.message}</p>
  }

  if (!basketItems || basketItems.length === 0) {
    return <p>Your shopping bag is empty.</p>
  }

  const lines = basketItems
    .map((line) => ({ line, item: catalogItems?.find((item) => item?.id === line.productId) }))
    .filter((pair): pair is { line: typeof pair.line; item: CatalogItem } => pair.item != null)

  const total = lines.reduce((sum, { line, item }) => sum + item.price * line.quantity, 0)

  return (
    <div className="cart-page">
      <table className="data-table">
        <thead>
          <tr>
            <th>Product</th>
            <th>Unit price</th>
            <th>Quantity</th>
            <th>Line total</th>
          </tr>
        </thead>
        <tbody>
          {lines.map(({ line, item }) => (
            <CartLineItem key={line.productId} item={item} quantity={line.quantity} onQuantityChange={(quantity) => setQuantity(line.productId, quantity)} />
          ))}
        </tbody>
      </table>
      <p>Total: ${total.toFixed(2)}</p>
      <Link to="/checkout" className="action">
        Checkout
      </Link>
    </div>
  )
}
