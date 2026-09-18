import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { apiFetch, ApiError } from '../lib/api'
import type { AuthResponse, AuthUser, LoginPayload, RegisterPayload } from '../types/auth'

const TOKEN_STORAGE_KEY = 'cozycinema.token'

interface AuthContextValue {
  user: AuthUser | null
  token: string | null
  status: 'loading' | 'authenticated' | 'unauthenticated'
  error: string | null
  login: (payload: LoginPayload) => Promise<void>
  register: (payload: RegisterPayload) => Promise<void>
  logout: () => void
}

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_STORAGE_KEY))
  const [user, setUser] = useState<AuthUser | null>(null)
  const [status, setStatus] = useState<'loading' | 'authenticated' | 'unauthenticated'>(
    token ? 'loading' : 'unauthenticated',
  )
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) {
      return
    }

    let cancelled = false

    apiFetch<AuthUser>('/api/auth/me', { token })
      .then((currentUser) => {
        if (!cancelled) {
          setUser(currentUser)
          setStatus('authenticated')
        }
      })
      .catch(() => {
        if (!cancelled) {
          localStorage.removeItem(TOKEN_STORAGE_KEY)
          setToken(null)
          setUser(null)
          setStatus('unauthenticated')
        }
      })

    return () => {
      cancelled = true
    }
  }, [token])

  const applyAuthResponse = useCallback((response: AuthResponse) => {
    localStorage.setItem(TOKEN_STORAGE_KEY, response.token)
    setToken(response.token)
    setUser(response.user)
    setStatus('authenticated')
  }, [])

  const login = useCallback(
    async (payload: LoginPayload) => {
      setError(null)
      try {
        const response = await apiFetch<AuthResponse>('/api/auth/login', {
          method: 'POST',
          body: JSON.stringify(payload),
        })
        applyAuthResponse(response)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível entrar.')
        throw err
      }
    },
    [applyAuthResponse],
  )

  const register = useCallback(
    async (payload: RegisterPayload) => {
      setError(null)
      try {
        const response = await apiFetch<AuthResponse>('/api/auth/register', {
          method: 'POST',
          body: JSON.stringify(payload),
        })
        applyAuthResponse(response)
      } catch (err) {
        setError(err instanceof ApiError ? err.message : 'Não foi possível criar a conta.')
        throw err
      }
    },
    [applyAuthResponse],
  )

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    setToken(null)
    setUser(null)
    setStatus('unauthenticated')
  }, [])

  const value = useMemo(
    () => ({ user, token, status, error, login, register, logout }),
    [user, token, status, error, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
