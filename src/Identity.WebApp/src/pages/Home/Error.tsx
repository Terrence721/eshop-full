import { useSearchParams } from 'react-router'
import { getHomeError } from '../../api/home'
import { useAsync } from '../../lib/useAsync'

function HomeError() {
  const [searchParams] = useSearchParams()
  const errorId = searchParams.get('errorId') ?? ''
  const { data, loading, error } = useAsync(() => getHomeError(errorId), [errorId])

  if (loading) {
    return <p>Loading...</p>
  }

  return (
    <div>
      <h1>Error</h1>
      {error ? (
        <p>Could not load error details: {error.message}</p>
      ) : data?.error ? (
        <pre>{JSON.stringify(data.error, null, 2)}</pre>
      ) : (
        <p>No error information is available.</p>
      )}
    </div>
  )
}

export default HomeError
