import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { getHomeError, type HomeError as HomeErrorData } from '../../api/home'

function HomeError() {
  const [searchParams] = useSearchParams()
  const errorId = searchParams.get('errorId') ?? ''
  const [data, setData] = useState<HomeErrorData | null>(null)
  const [fetchError, setFetchError] = useState<Error | null>(null)

  useEffect(() => {
    getHomeError(errorId).then(setData).catch(setFetchError)
  }, [errorId])

  return (
    <div>
      <h1>Error</h1>
      {fetchError ? (
        <p>Could not load error details: {fetchError.message}</p>
      ) : data?.error ? (
        <pre>{JSON.stringify(data.error, null, 2)}</pre>
      ) : (
        <p>No error information is available.</p>
      )}
    </div>
  )
}

export default HomeError
