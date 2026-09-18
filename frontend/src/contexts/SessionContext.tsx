import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { apiFetch, ApiError } from '../lib/api'
import { useAuth } from '../hooks/useAuth'
import type {
  CreateSessionPayload,
  JoinSessionPayload,
  SelectMoviePayload,
  Session,
  SessionSummary,
} from '../types/session'

const SESSION_ID_STORAGE_KEY = 'cozycinema.sessionId'

interface SessionContextValue {
  session: Session | null
  status: 'idle' | 'loading' | 'ready' | 'error'
  error: string | null
  mySessions: SessionSummary[]
  mySessionsStatus: 'idle' | 'loading' | 'ready' | 'error'
  loadMySessions: () => Promise<void>
  createSession: (payload: CreateSessionPayload) => Promise<void>
  joinSession: (payload: JoinSessionPayload) => Promise<void>
  selectSession: (sessionId: string) => Promise<void>
  selectMovie: (payload: SelectMoviePayload) => Promise<void>
  refresh: () => Promise<void>
  switchSession: () => void
}

// eslint-disable-next-line react-refresh/only-export-components
export const SessionContext = createContext<SessionContextValue | null>(null)

export function SessionProvider({ children }: { children: ReactNode }) {
  const { token } = useAuth()
  const [sessionId, setSessionId] = useState<string | null>(() =>
    localStorage.getItem(SESSION_ID_STORAGE_KEY),
  )
  const [session, setSession] = useState<Session | null>(null)
  const [status, setStatus] = useState<'idle' | 'loading' | 'ready' | 'error'>(
    sessionId ? 'loading' : 'idle',
  )
  const [error, setError] = useState<string | null>(null)
  const [mySessions, setMySessions] = useState<SessionSummary[]>([])
  const [mySessionsStatus, setMySessionsStatus] = useState<'idle' | 'loading' | 'ready' | 'error'>(
    'idle',
  )

  const applySession = useCallback((next: Session) => {
    localStorage.setItem(SESSION_ID_STORAGE_KEY, next.id)
    setSessionId(next.id)
    setSession(next)
    setStatus('ready')
  }, [])

  useEffect(() => {
    if (!token || !sessionId) {
      return
    }

    let cancelled = false

    apiFetch<Session>(`/api/sessions/${sessionId}`, { token })
      .then((current) => {
        if (!cancelled) {
          setSession(current)
          setStatus('ready')
        }
      })
      .catch(() => {
        if (!cancelled) {
          localStorage.removeItem(SESSION_ID_STORAGE_KEY)
          setSessionId(null)
          setSession(null)
          setStatus('idle')
        }
      })

    return () => {
      cancelled = true
    }
  }, [token, sessionId])

  const loadMySessions = useCallback(async () => {
    if (!token) {
      return
    }

    setMySessionsStatus('loading')
    try {
      const sessions = await apiFetch<SessionSummary[]>('/api/sessions', { token })
      setMySessions(sessions)
      setMySessionsStatus('ready')
    } catch {
      setMySessionsStatus('error')
    }
  }, [token])

  const selectSession = useCallback(
    async (targetSessionId: string) => {
      setError(null)
      try {
        const next = await apiFetch<Session>(`/api/sessions/${targetSessionId}`, { token })
        applySession(next)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível entrar na sessão.')
        throw err
      }
    },
    [token, applySession],
  )

  const createSession = useCallback(
    async (payload: CreateSessionPayload) => {
      setError(null)
      try {
        const next = await apiFetch<Session>('/api/sessions', {
          method: 'POST',
          body: JSON.stringify(payload),
          token,
        })
        applySession(next)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível criar a sessão.')
        throw err
      }
    },
    [token, applySession],
  )

  const joinSession = useCallback(
    async (payload: JoinSessionPayload) => {
      setError(null)
      try {
        const next = await apiFetch<Session>('/api/sessions/join', {
          method: 'POST',
          body: JSON.stringify(payload),
          token,
        })
        applySession(next)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível entrar na sessão.')
        throw err
      }
    },
    [token, applySession],
  )

  const selectMovie = useCallback(
    async (payload: SelectMoviePayload) => {
      if (!sessionId) {
        return
      }

      setError(null)
      try {
        const next = await apiFetch<Session>(`/api/sessions/${sessionId}/casal/movie-pick`, {
          method: 'POST',
          body: JSON.stringify(payload),
          token,
        })
        applySession(next)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível escolher o filme.')
        throw err
      }
    },
    [token, sessionId, applySession],
  )

  const refresh = useCallback(async () => {
    if (!sessionId) {
      return
    }

    const current = await apiFetch<Session>(`/api/sessions/${sessionId}`, { token })
    setSession(current)
  }, [token, sessionId])

  const switchSession = useCallback(() => {
    // Apenas troca a sessão ativa no cliente — o usuário continua membro dela e pode
    // selecioná-la de novo na lista (`mySessions`) sem precisar do código de convite.
    localStorage.removeItem(SESSION_ID_STORAGE_KEY)
    setSessionId(null)
    setSession(null)
    setStatus('idle')
  }, [])

  const value = useMemo(
    () => ({
      session,
      status,
      error,
      mySessions,
      mySessionsStatus,
      loadMySessions,
      createSession,
      joinSession,
      selectSession,
      selectMovie,
      refresh,
      switchSession,
    }),
    [
      session,
      status,
      error,
      mySessions,
      mySessionsStatus,
      loadMySessions,
      createSession,
      joinSession,
      selectSession,
      selectMovie,
      refresh,
      switchSession,
    ],
  )

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}
