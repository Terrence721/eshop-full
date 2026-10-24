import { catalogItemPictureUrl, type CatalogItem } from '../api/catalog'

interface CartLineItemProps {
  item: CatalogItem
  quantity: number
  onQuantityChange: (quantity: number) => void
}

// Mirrors Pages/Cart/CartPage.razor's per-line quantity form. Setting the
// quantity to 0 removes the line, matching upstream's own
// BasketState.SetQuantityAsync semantics.
export function CartLineItem({ item, quantity, onQuantityChange }: CartLineItemProps) {
  return (
    <tr>
      <td>
        <img src={catalogItemPictureUrl(item.id)} alt={item.name} />
        {item.name}
      </td>
      <td>${item.price.toFixed(2)}</td>
      <td>
        <input
          type="number"
          min={0}
          aria-label="product quantity"
          value={quantity}
          onChange={(e) => onQuantityChange(Number(e.target.value))}
        />
      </td>
      <td>${(item.price * quantity).toFixed(2)}</td>
    </tr>
  )
}
