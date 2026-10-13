import { clearBasket, getBasket, updateBasket, type BasketQuantity } from '../api/basket'
import { useAsync } from './useAsync'

interface BasketState {
  items: BasketQuantity[] | null
  loading: boolean
  error: Error | null
  addItem: (productId: number) => Promise<void>
  setQuantity: (productId: number, quantity: number) => Promise<void>
  clear: () => Promise<void>
}

// Every mutation resends the whole basket in one PUT, matching
// UpdateBasketRequest's full-replace gRPC semantics (there is no
// single-line-update RPC to call instead) -- and updates local state
// from the response WebBFF actually persisted, not an optimistic guess.
export function useBasket(): BasketState {
  const { data: items, loading, error, setData } = useAsync(getBasket, [])

  async function replaceWith(nextItems: BasketQuantity[]) {
    const updated = await updateBasket(nextItems)
    setData(updated)
  }

  async function addItem(productId: number) {
    const current = items ?? []
    const existing = current.find((item) => item.productId === productId)
    const nextItems = existing
      ? current.map((item) => (item.productId === productId ? { ...item, quantity: item.quantity + 1 } : item))
      : [...current, { productId, quantity: 1 }]
    await replaceWith(nextItems)
  }

  async function setQuantity(productId: number, quantity: number) {
    const current = items ?? []
    const nextItems =
      quantity <= 0 ? current.filter((item) => item.productId !== productId) : current.map((item) => (item.productId === productId ? { ...item, quantity } : item))
    await replaceWith(nextItems)
  }

  async function clear() {
    await clearBasket()
    setData([])
  }

  return { items, loading, error, addItem, setQuantity, clear }
}
