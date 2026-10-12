import { Link } from 'react-router'
import { RegisteredHooks } from '../../components/RegisteredHooks'
import { ReceivedMessages } from '../../components/ReceivedMessages'
import { useCurrentUser } from '../../lib/useCurrentUser'

// Mirrors Home.razor's <AuthorizeView> -- RegisteredHooks only renders (and
// the add-webhook link only shows) once the visitor is actually signed in;
// an anonymous visitor sees the same placeholder upstream shows.
function HomeIndex() {
  const { claims, loading } = useCurrentUser()

  return (
    <>
      <h2>Registered webhooks</h2>
      {loading ? null : claims ? (
        <>
          <RegisteredHooks />
          <Link className="action" to="/add-webhook">
            Add webhook registration
          </Link>
        </>
      ) : (
        <div className="grid-placeholder">Log in to view or edit webhook registrations</div>
      )}

      <h2>Webhook messages received (orders paid)</h2>
      <ReceivedMessages />
    </>
  )
}

export default HomeIndex
