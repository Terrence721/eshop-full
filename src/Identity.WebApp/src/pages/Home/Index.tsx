import { getHomeIndex } from '../../api/home'
import { useAsync } from '../../lib/useAsync'

function HomeIndex() {
  const { data, loading, error } = useAsync(getHomeIndex, [])

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p>Could not load this page: {error.message}</p>
  }

  if (!data) {
    return <p>This page is only available in Development.</p>
  }

  return (
    <div>
      <h1>Identity.WebApp</h1>
      <p>Duende IdentityServer version {data.version}</p>
      <ul>
        <li>
          <a href={data.wellKnownConfigurationUrl}>OpenID Connect discovery document</a>
        </li>
        <li>
          <a href={data.diagnosticsUrl}>Diagnostics</a>
        </li>
        <li>
          <a href={data.grantsUrl}>Grants</a>
        </li>
      </ul>
    </div>
  )
}

export default HomeIndex
