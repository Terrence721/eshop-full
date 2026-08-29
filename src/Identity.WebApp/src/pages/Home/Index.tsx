import { useEffect, useState } from 'react'
import { getHomeIndex, type HomeIndex as HomeIndexData } from '../../api/home'

function HomeIndex() {
  const [data, setData] = useState<HomeIndexData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<Error | null>(null)

  useEffect(() => {
    getHomeIndex()
      .then(setData)
      .catch(setError)
      .finally(() => setLoading(false))
  }, [])

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
