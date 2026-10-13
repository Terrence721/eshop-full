import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router'
import { getCatalogItem, type CatalogItem } from '../../api/catalog'
import { clearBasket } from '../../api/basket'
import { createOrder, type OrderBasketItem } from '../../api/orders'
import { useAsync } from '../../lib/useAsync'
import { useBasket } from '../../lib/useBasket'
import { useCurrentUser } from '../../lib/useCurrentUser'

function claimValue(claims: { type: string; value: string }[] | null, type: string) {
  return claims?.find((claim) => claim.type === type)?.value ?? ''
}

// Mirrors Pages/Checkout/Checkout.razor. Address fields are pre-filled
// from the real OIDC address claims (confirmed in
// src/Identity.API/Services/ProfileService.cs) but stay editable, matching
// upstream's own EditForm. Card details are hardcoded fake data, same as
// upstream's own stub -- this was never a real payment integration.
export default function CheckoutIndex() {
  const { claims } = useCurrentUser()
  const { items: basketItems, loading: basketLoading, error: basketError } = useBasket()
  const { data: catalogItems, loading: catalogLoading, error: catalogError } = useAsync(
    async () => {
      if (!basketItems || basketItems.length === 0) {
        return [] as CatalogItem[]
      }
      return Promise.all(basketItems.map((line) => getCatalogItem(line.productId)))
    },
    [basketItems],
  )
  const navigate = useNavigate()
  const [address, setAddress] = useState({
    street: claimValue(claims, 'address_street'),
    city: claimValue(claims, 'address_city'),
    state: claimValue(claims, 'address_state'),
    country: claimValue(claims, 'address_country'),
    zipCode: claimValue(claims, 'address_zip_code'),
  })
  const [submitting, setSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<Error | null>(null)

  if (basketLoading || catalogLoading) {
    return <p>Loading...</p>
  }

  if (basketError || catalogError) {
    return <p className="error-message">{(basketError ?? catalogError)?.message}</p>
  }

  if (!basketItems || basketItems.length === 0) {
    return <p>Your shopping bag is empty.</p>
  }

  const lines: OrderBasketItem[] = basketItems
    .map((line) => {
      const item = catalogItems?.find((catalogItem) => catalogItem?.id === line.productId)
      if (!item) {
        return null
      }
      return { productId: item.id, productName: item.name, unitPrice: item.price, oldUnitPrice: 0, quantity: line.quantity, pictureUrl: `/remoteapi/catalog/api/catalog/items/${item.id}/pic` }
    })
    .filter((line): line is OrderBasketItem => line != null)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setSubmitting(true)
    setSubmitError(null)
    try {
      const userId = claimValue(claims, 'sub')
      await createOrder(
        {
          userId,
          userName: claimValue(claims, 'name'),
          ...address,
          cardNumber: '1111222233334444',
          cardHolderName: 'TESTUSER',
          cardExpiration: '2030-01-01',
          cardSecurityNumber: '123',
          cardTypeId: 1,
          buyer: userId,
          items: lines,
        },
        crypto.randomUUID(),
      )
      await clearBasket()
      navigate('/user/orders')
    } catch (err) {
      setSubmitError(err instanceof Error ? err : new Error(String(err)))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form className="checkout-page" onSubmit={handleSubmit}>
      <h2>Checkout</h2>
      {submitError && <p className="error-message">{submitError.message}</p>}
      <label>
        Street
        <input type="text" required value={address.street} onChange={(e) => setAddress({ ...address, street: e.target.value })} />
      </label>
      <label>
        City
        <input type="text" required value={address.city} onChange={(e) => setAddress({ ...address, city: e.target.value })} />
      </label>
      <label>
        State
        <input type="text" required value={address.state} onChange={(e) => setAddress({ ...address, state: e.target.value })} />
      </label>
      <label>
        Country
        <input type="text" required value={address.country} onChange={(e) => setAddress({ ...address, country: e.target.value })} />
      </label>
      <label>
        Zip code
        <input type="text" required value={address.zipCode} onChange={(e) => setAddress({ ...address, zipCode: e.target.value })} />
      </label>
      <button className="action" type="submit" disabled={submitting}>
        Place order
      </button>
    </form>
  )
}
