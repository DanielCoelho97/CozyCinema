import { createContext, useEffect, useState, type ReactNode } from 'react'
import type { HubConnection } from '@microsoft/signalr'
import { createCinemaSessionConnection } from '../lib/signalr'
import { useAuth } from '../hooks/useAuth'
import { useSession } from '../hooks/useSession'

const API_URL = import.meta.env.VITE_API_URL as string

interface RealtimeContextValue {
  connection: HubConnection | null
}

// eslint-disable-next-line react-refresh/only-export-components
export const RealtimeContext = createContext<RealtimeContextValue>({ connection: null })

export function RealtimeProvider({ children }: { children: ReactNode }) {
  const { token } = useAuth()
  const { session } = useSession()
  const sessionId = session?.id ?? null
  const [connection, setConnection] = useState<HubConnection | null>(null)

  useEffect(() => {
    if (!token || !sessionId) {
      return
    }

    const hubConnection = createCinemaSessionConnection(API_URL, sessionId, token)
    let cancelled = false

    hubConnection
      .start()
      .then(() => {
        if (!cancelled) {
          setConnection(hubConnection)
        }
      })
      .catch(() => {
        // a reconexão automática do SignalR tenta de novo sozinha; o estado local segue sem conexão até lá
      })

    return () => {
      cancelled = true
      setConnection(null)
      hubConnection.stop()
    }
  }, [token, sessionId])

  return <RealtimeContext.Provider value={{ connection }}>{children}</RealtimeContext.Provider>
}
