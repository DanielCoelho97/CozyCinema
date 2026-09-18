import { useCallback, useState } from 'react'
import { apiFetch, ApiError } from '../lib/api'
import { useAuth } from './useAuth'
import { useSession } from './useSession'
import type { LogWatchedMoviePayload } from '../types/movieLog'

export function useMovieLogActions(sessionId: string) {
  const { token } = useAuth()
  const { refresh } = useSession()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const markCurrentMovieWatched = useCallback(async () => {
    setIsSubmitting(true)
    setError(null)
    try {
      await apiFetch(`/api/movie-log/${sessionId}/from-current`, {
        method: 'POST',
        body: JSON.stringify({}),
        token,
      })
      await refresh()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível marcar o filme como assistido.')
      throw err
    } finally {
      setIsSubmitting(false)
    }
  }, [token, sessionId, refresh])

  const logWatchedMovie = useCallback(
    async (payload: LogWatchedMoviePayload) => {
      setIsSubmitting(true)
      setError(null)
      try {
        await apiFetch(`/api/movie-log/${sessionId}/manual`, {
          method: 'POST',
          body: JSON.stringify(payload),
          token,
        })
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível registrar o filme assistido.')
        throw err
      } finally {
        setIsSubmitting(false)
      }
    },
    [token, sessionId],
  )

  return { markCurrentMovieWatched, logWatchedMovie, isSubmitting, error }
}
