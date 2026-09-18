import { useCallback, useEffect, useState } from 'react'
import { apiFetch, ApiError } from '../lib/api'
import { useAuth } from './useAuth'
import type { MovieLogFilters, WatchedMovie } from '../types/movieLog'

const DEFAULT_FILTERS: MovieLogFilters = {
  sortBy: 'watchedAt',
  sortDirection: 'desc',
  genre: '',
  director: '',
}

interface UseMovieLogOptions {
  sessionId?: string
}

export function useMovieLog({ sessionId }: UseMovieLogOptions) {
  const { token } = useAuth()
  const [filters, setFilters] = useState<MovieLogFilters>(DEFAULT_FILTERS)
  const [movies, setMovies] = useState<WatchedMovie[]>([])
  const [status, setStatus] = useState<'loading' | 'ready' | 'error'>('loading')
  const [error, setError] = useState<string | null>(null)

  const buildPath = useCallback(() => {
    const params = new URLSearchParams({
      sortBy: filters.sortBy,
      sortDirection: filters.sortDirection,
    })

    if (filters.genre.trim()) {
      params.set('genre', filters.genre.trim())
    }

    if (filters.director.trim()) {
      params.set('director', filters.director.trim())
    }

    return sessionId
      ? `/api/movie-log/session/${sessionId}?${params.toString()}`
      : `/api/movie-log/me?${params.toString()}`
  }, [sessionId, filters])

  const refresh = useCallback(async () => {
    setStatus('loading')
    setError(null)

    try {
      const results = await apiFetch<WatchedMovie[]>(buildPath(), { token })
      setMovies(results)
      setStatus('ready')
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível carregar o histórico.')
      setStatus('error')
    }
  }, [token, buildPath])

  useEffect(() => {
    let cancelled = false

    apiFetch<WatchedMovie[]>(buildPath(), { token })
      .then((results) => {
        if (!cancelled) {
          setMovies(results)
          setStatus('ready')
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : 'Não foi possível carregar o histórico.')
          setStatus('error')
        }
      })

    return () => {
      cancelled = true
    }
  }, [token, buildPath])

  const applyFilters = useCallback((next: MovieLogFilters) => {
    setStatus('loading')
    setError(null)
    setFilters(next)
  }, [])

  return { movies, status, error, filters, setFilters: applyFilters, refresh }
}
