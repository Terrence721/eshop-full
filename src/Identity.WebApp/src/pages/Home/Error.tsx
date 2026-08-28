import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router'
import { getHomeError, type HomeError as HomeErrorData } from '../../api/home'

function HomeError() {
  const [searchParams] = useSearchParams()
  const errorId = searchParams.get('errorId') ?? ''
  const [data, setData] = useState<HomeErrorData | null>(null)

  useEffect(() => {
    getHomeError(errorId).then(setData)
  }, [errorId])

  return (
    <div>
      <h1>Error</h1>
      {data?.error ? (
        <pre>{JSON.stringify(data.error, null, 2)}</pre>
      ) : (
        <p>No error information is available.</p>
      )}
    </div>
  )
}

export default HomeError
