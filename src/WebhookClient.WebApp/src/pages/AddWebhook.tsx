import { useState, type SubmitEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { getDefaultToken, registerHook } from '../api/hooks'
import { useAsync } from '../lib/useAsync'

// Mirrors AddWebhook.razor. GrantUrl/Url/Event are no longer this page's
// concern -- HooksApiEndpoints.cs builds them server-side now (see
// hooks.ts's registerHook), so this only ever sends the token.
function AddWebhook() {
  const navigate = useNavigate()
  const [token, setToken] = useState('')
  const [message, setMessage] = useState<string | null>(null)

  useAsync(getDefaultToken, [], (result) => {
    if (result.token) {
      setToken(result.token)
    }
  })

  async function register(event: SubmitEvent) {
    event.preventDefault()
    if (!token) {
      return
    }

    setMessage(null)
    const response = await registerHook(token)
    if (response.ok) {
      navigate('/')
    } else {
      // "Registation" matches AddWebhook.razor's own message verbatim.
      setMessage(`Registation was rejected with status ${response.status}`)
    }
  }

  return (
    <>
      <h2>Register a new webhook</h2>
      <p>
        This page registers the "OrderPaid" Webhook by sending a POST to the WebHooks API. Once the Webhook is set, you will be able to
        see new paid orders from the <Link to="/">home</Link> page.
      </p>

      <form onSubmit={register}>
        <p>
          <label>
            Token:
            <input type="text" value={token} onChange={(event) => setToken(event.target.value)} placeholder="Token" />
          </label>
        </p>
        <button className="action" type="submit">
          Register
        </button>
      </form>

      {message && <p className="error-message">{message}</p>}
    </>
  )
}

export default AddWebhook
