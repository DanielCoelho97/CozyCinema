import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { apiFetch, ApiError } from '../lib/api'
import { useAuth } from '../hooks/useAuth'
import { useRealtime } from '../hooks/useRealtime'
import { useHubEvent } from '../hooks/useHubEvent'
import { SessionHubEvent } from '../lib/sessionHubEvents'
import type { CastVotePayload, SuggestMoviePayload, VotingRound } from '../types/voting'

interface VotingRoundContextValue {
  round: VotingRound | null
  status: 'idle' | 'loading' | 'ready' | 'error'
  error: string | null
  suggestMovie: (payload: SuggestMoviePayload) => Promise<void>
  markReady: () => Promise<void>
  castVote: (payload: CastVotePayload) => Promise<void>
  refreshRound: () => Promise<void>
}

// eslint-disable-next-line react-refresh/only-export-components
export const VotingRoundContext = createContext<VotingRoundContextValue | null>(null)

interface VotingRoundProviderProps {
  sessionId: string
  children: ReactNode
}

export function VotingRoundProvider({ sessionId, children }: VotingRoundProviderProps) {
  const { token } = useAuth()
  const { connection } = useRealtime()
  const [round, setRound] = useState<VotingRound | null>(null)
  const [status, setStatus] = useState<'idle' | 'loading' | 'ready' | 'error'>('loading')
  const [error, setError] = useState<string | null>(null)

  const refreshRound = useCallback(async () => {
    const current = await apiFetch<VotingRound | null>(`/api/sessions/${sessionId}/grupo/round`, {
      token,
    })
    setRound(current)
    setStatus('ready')
  }, [token, sessionId])

  useEffect(() => {
    let cancelled = false

    apiFetch<VotingRound | null>(`/api/sessions/${sessionId}/grupo/round`, { token })
      .then((current) => {
        if (!cancelled) {
          setRound(current)
          setStatus('ready')
        }
      })
      .catch(() => {
        if (!cancelled) {
          setStatus('error')
        }
      })

    return () => {
      cancelled = true
    }
  }, [token, sessionId])

  const handleRoundEvent = useCallback(() => {
    refreshRound().catch(() => {
      // mantém o último estado conhecido; o próximo evento do hub tenta de novo
    })
  }, [refreshRound])

  useHubEvent(connection, SessionHubEvent.MovieSuggested, handleRoundEvent)
  useHubEvent(connection, SessionHubEvent.ReadyToVote, handleRoundEvent)
  useHubEvent(connection, SessionHubEvent.VotingStarted, handleRoundEvent)
  useHubEvent(connection, SessionHubEvent.VoteSubmitted, handleRoundEvent)
  useHubEvent(connection, SessionHubEvent.VotingCompleted, handleRoundEvent)
  useHubEvent(connection, SessionHubEvent.MovieWatched, handleRoundEvent)

  const suggestMovie = useCallback(
    async (payload: SuggestMoviePayload) => {
      setError(null)
      try {
        const next = await apiFetch<VotingRound>(`/api/sessions/${sessionId}/grupo/movies`, {
          method: 'POST',
          body: JSON.stringify(payload),
          token,
        })
        setRound(next)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível sugerir o filme.')
        throw err
      }
    },
    [token, sessionId],
  )

  const markReady = useCallback(async () => {
    setError(null)
    try {
      const next = await apiFetch<VotingRound>(`/api/sessions/${sessionId}/grupo/ready`, {
        method: 'POST',
        token,
      })
      setRound(next)
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Não foi possível sinalizar prontidão.')
      throw err
    }
  }, [token, sessionId])

  const castVote = useCallback(
    async (payload: CastVotePayload) => {
      setError(null)
      try {
        const next = await apiFetch<VotingRound>(`/api/sessions/${sessionId}/grupo/votes`, {
          method: 'POST',
          body: JSON.stringify(payload),
          token,
        })
        setRound(next)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível registrar o voto.')
        throw err
      }
    },
    [token, sessionId],
  )

  const value = useMemo(
    () => ({ round, status, error, suggestMovie, markReady, castVote, refreshRound }),
    [round, status, error, suggestMovie, markReady, castVote, refreshRound],
  )

  return <VotingRoundContext.Provider value={value}>{children}</VotingRoundContext.Provider>
}
