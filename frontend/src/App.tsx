import { useState } from 'react'
import { useAuth } from './hooks/useAuth'
import { AuthScreen } from './components/auth/AuthScreen'
import { SessionProvider } from './contexts/SessionContext'
import { RealtimeProvider } from './contexts/RealtimeContext'
import { useSession } from './hooks/useSession'
import { SessionHome } from './components/sessions/SessionHome'
import { CasalSessionScreen } from './components/sessions/casal/CasalSessionScreen'
import { GrupoSessionScreen } from './components/sessions/grupo/GrupoSessionScreen'
import { SessionRealtimeBridge } from './components/realtime/SessionRealtimeBridge'
import { MovieLogScreen } from './components/movielog/MovieLogScreen'

function SessionRouter() {
  const { session, status } = useSession()
  const [showMovieLog, setShowMovieLog] = useState(false)

  if (status === 'loading') {
    return (
      <main className="flex min-h-svh flex-col items-center justify-center bg-cinema-background px-6 text-center">
        <p className="text-sm text-cinema-textMuted">Carregando sessão…</p>
      </main>
    )
  }

  if (!session) {
    return <SessionHome />
  }

  if (showMovieLog) {
    return <MovieLogScreen sessionId={session.id} onClose={() => setShowMovieLog(false)} />
  }

  if (session.mode === 'Casal') {
    return <CasalSessionScreen onOpenMovieLog={() => setShowMovieLog(true)} />
  }

  return <GrupoSessionScreen onOpenMovieLog={() => setShowMovieLog(true)} />
}

function AuthenticatedApp() {
  return (
    <SessionProvider>
      <RealtimeProvider>
        <SessionRealtimeBridge />
        <SessionRouter />
      </RealtimeProvider>
    </SessionProvider>
  )
}

function App() {
  const { status } = useAuth()

  if (status === 'loading') {
    return (
      <main className="flex min-h-svh flex-col items-center justify-center bg-cinema-background px-6 text-center">
        <p className="text-sm text-cinema-textMuted">Carregando…</p>
      </main>
    )
  }

  return status === 'authenticated' ? <AuthenticatedApp /> : <AuthScreen />
}

export default App
