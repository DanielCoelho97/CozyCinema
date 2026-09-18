import { useEffect, useState } from 'react'
import { useRealtime } from '../../hooks/useRealtime'
import { useSession } from '../../hooks/useSession'
import { useHubEvent } from '../../hooks/useHubEvent'
import { SessionHubEvent, type SessionErrorPayload } from '../../lib/sessionHubEvents'

const ERROR_DISPLAY_MS = 4000

export function SessionRealtimeBridge() {
  const { connection } = useRealtime()
  const { refresh } = useSession()
  const [sessionError, setSessionError] = useState<SessionErrorPayload | null>(null)

  useHubEvent(connection, SessionHubEvent.UserJoinedSession, () => {
    refresh().catch(() => {})
  })

  useHubEvent(connection, SessionHubEvent.UserLeftSession, () => {
    refresh().catch(() => {})
  })

  useHubEvent(connection, SessionHubEvent.MovieSelected, () => {
    refresh().catch(() => {})
  })

  useHubEvent(connection, SessionHubEvent.TurnPassed, () => {
    refresh().catch(() => {})
  })

  useHubEvent(connection, SessionHubEvent.MovieWatched, () => {
    refresh().catch(() => {})
  })

  useHubEvent<SessionErrorPayload>(connection, SessionHubEvent.SessionError, (payload) => {
    setSessionError(payload)
  })

  useEffect(() => {
    if (!sessionError) {
      return
    }

    const timeout = setTimeout(() => setSessionError(null), ERROR_DISPLAY_MS)
    return () => clearTimeout(timeout)
  }, [sessionError])

  if (!sessionError) {
    return null
  }

  return (
    <div className="fixed inset-x-0 top-0 z-50 flex justify-center px-4 pt-[max(1rem,env(safe-area-inset-top))]">
      <div className="rounded-2xl bg-red-500/90 px-4 py-2 text-center text-sm font-medium text-white shadow-cozy">
        {sessionError.message}
      </div>
    </div>
  )
}
