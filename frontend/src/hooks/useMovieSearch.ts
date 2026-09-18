import { useEffect, useState } from 'react'
import { apiFetch, ApiError } from '../lib/api'
import { useAuth } from './useAuth'
import type { MovieSummary } from '../types/movie'

const DEBOUNCE_MS = 400

export function useMovieSearch() {
  const { token } = useAuth()
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<MovieSummary[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const trimmed = query.trim()
    let cancelled = false

    const timeoutId = setTimeout(
      () => {
        if (cancelled) {
          return
        }

        if (!trimmed) {
          setResults([])
          setError(null)
          setIsLoading(false)
          return
        }

        setIsLoading(true)
        setError(null)

        apiFetch<MovieSummary[]>(`/api/movies/search?query=${encodeURIComponent(trimmed)}`, { token })
          .then((next) => {
            if (!cancelled) {
              setResults(next)
            }
          })
          .catch((err) => {
            if (!cancelled) {
              setError(err instanceof ApiError ? err.message : 'Não foi possível buscar filmes.')
              setResults([])
            }
          })
          .finally(() => {
            if (!cancelled) {
              setIsLoading(false)
            }
          })
      },
      trimmed ? DEBOUNCE_MS : 0,
    )

    return () => {
      cancelled = true
      clearTimeout(timeoutId)
    }
  }, [query, token])

  return { query, setQuery, results, isLoading, error }
}
