import { useEffect, useState } from 'react'
import { motion, AnimatePresence, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../lib/motion'
import { useAuth } from '../../hooks/useAuth'
import { useSession } from '../../hooks/useSession'
import { CreateSessionForm } from './CreateSessionForm'
import { JoinSessionForm } from './JoinSessionForm'

function SessionPickerList() {
  const { mySessions, mySessionsStatus, loadMySessions, selectSession } = useSession()
  const [enteringId, setEnteringId] = useState<string | null>(null)

  useEffect(() => {
    loadMySessions()
  }, [loadMySessions])

  if (mySessionsStatus === 'loading' && mySessions.length === 0) {
    return <p className="text-center text-sm text-cinema-textMuted">Carregando suas sessões…</p>
  }

  if (mySessions.length === 0) {
    return null
  }

  const handleSelect = async (id: string) => {
    setEnteringId(id)
    try {
      await selectSession(id)
    } finally {
      setEnteringId(null)
    }
  }

  return (
    <div className="flex w-full max-w-sm flex-col gap-3 sm:max-w-md">
      <p className="text-sm font-medium text-cinema-textMuted">Suas sessões</p>
      <div className="flex flex-col gap-2">
        {mySessions.map((summary) => (
          <button
            key={summary.id}
            type="button"
            onClick={() => handleSelect(summary.id)}
            disabled={enteringId !== null}
            className="flex items-center justify-between gap-3 rounded-2xl bg-cinema-surfaceElevated px-4 py-3 text-left shadow-cozy transition hover:bg-cinema-primaryHover disabled:opacity-60"
          >
            <span className="min-w-0 flex-1">
              <span className="block truncate text-sm font-semibold text-cinema-text">
                {summary.title}
              </span>
              <span className="block text-xs text-cinema-textMuted">
                {summary.mode === 'Casal' ? 'Casal' : 'Grupo'} · {summary.activeMemberCount}{' '}
                {summary.activeMemberCount === 1 ? 'membro' : 'membros'}
              </span>
            </span>
            <span className="flex-none text-xs font-semibold text-cinema-accent">
              {enteringId === summary.id ? 'Entrando…' : 'Entrar'}
            </span>
          </button>
        ))}
      </div>
    </div>
  )
}

export function SessionHome() {
  const { logout } = useAuth()
  const [mode, setMode] = useState<'create' | 'join'>('create')
  const shouldReduceMotion = useReducedMotion()

  return (
    <main className="relative flex min-h-svh flex-col items-center justify-center gap-8 bg-cinema-background px-6 py-12">
      <button
        type="button"
        onClick={logout}
        className="absolute right-6 top-6 rounded-lg px-2 py-1 text-sm font-medium text-cinema-textMuted transition hover:text-cinema-accent"
      >
        Sair
      </button>

      <motion.h1
        {...cozyMotion(shouldReduceMotion, {
          initial: { opacity: 0, y: -8 },
          animate: { opacity: 1, y: 0 },
          transition: { duration: 0.35, ease: 'easeInOut' },
        })}
        className="font-heading text-2xl font-bold text-cinema-text"
      >
        Que filme hoje?
      </motion.h1>

      <SessionPickerList />

      <div className="w-full max-w-sm rounded-3xl bg-cinema-surfaceElevated p-6 shadow-cozyLg sm:max-w-md">
        <AnimatePresence mode="wait">
          {mode === 'create' ? (
            <motion.div
              key="create"
              {...cozyMotion(shouldReduceMotion, {
                initial: { opacity: 0, x: -8 },
                animate: { opacity: 1, x: 0 },
                exit: { opacity: 0, x: 8 },
                transition: { duration: 0.25, ease: 'easeInOut' },
              })}
            >
              <CreateSessionForm onSwitchToJoin={() => setMode('join')} />
            </motion.div>
          ) : (
            <motion.div
              key="join"
              {...cozyMotion(shouldReduceMotion, {
                initial: { opacity: 0, x: 8 },
                animate: { opacity: 1, x: 0 },
                exit: { opacity: 0, x: -8 },
                transition: { duration: 0.25, ease: 'easeInOut' },
              })}
            >
              <JoinSessionForm onSwitchToCreate={() => setMode('create')} />
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </main>
  )
}
