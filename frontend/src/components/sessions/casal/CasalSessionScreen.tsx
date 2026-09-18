import { useState } from 'react'
import { motion, AnimatePresence, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../../lib/motion'
import { useAuth } from '../../../hooks/useAuth'
import { useSession } from '../../../hooks/useSession'
import { MoviePickForm } from './MoviePickForm'
import { MarkWatchedSheet } from '../../movielog/MarkWatchedSheet'

interface CasalSessionScreenProps {
  onOpenMovieLog: () => void
}

export function CasalSessionScreen({ onOpenMovieLog }: CasalSessionScreenProps) {
  const { user } = useAuth()
  const { session, switchSession } = useSession()
  const [isPicking, setIsPicking] = useState(false)
  const [isMarkingWatched, setIsMarkingWatched] = useState(false)
  const shouldReduceMotion = useReducedMotion()

  if (!session) {
    return null
  }

  const nextPicker = session.members.find((member) => member.userId === session.nextPickerUserId)
  const isMyTurn = session.nextPickerUserId === user?.id
  const canPickNext = isMyTurn && !session.currentMovie

  return (
    <main className="mx-auto flex min-h-svh w-full max-w-2xl flex-col bg-cinema-background px-6 pb-32 pt-12">
      <header className="mb-8 flex flex-col items-center gap-1 text-center">
        <h1 className="font-heading text-2xl font-bold text-cinema-text">{session.title}</h1>
        <p className="text-sm text-cinema-textMuted">
          Código de convite:{' '}
          <span className="font-semibold tracking-[0.3em] text-cinema-accent">{session.inviteCode}</span>
        </p>
        <div className="mt-2 flex gap-4">
          <button
            type="button"
            onClick={onOpenMovieLog}
            className="rounded-lg px-2 py-1 text-xs font-medium text-cinema-textMuted transition hover:text-cinema-accent"
          >
            Histórico
          </button>
          <button
            type="button"
            onClick={switchSession}
            className="rounded-lg px-2 py-1 text-xs font-medium text-cinema-textMuted transition hover:text-cinema-accent"
          >
            Trocar de sessão
          </button>
        </div>
      </header>

      <div className="flex flex-1 flex-col items-center gap-6">
        <AnimatePresence mode="wait">
          <motion.div
            key={session.nextPickerUserId ?? 'aguardando'}
            {...cozyMotion(shouldReduceMotion, {
              initial: { opacity: 0, scale: 1.04 },
              animate: { opacity: 1, scale: 1 },
              exit: { opacity: 0, scale: 0.96 },
              transition: { duration: 0.35, ease: 'easeInOut' },
            })}
            className={`rounded-2xl px-6 py-3 text-center text-base font-semibold shadow-cozy ${
              isMyTurn
                ? 'bg-cinema-accent text-cinema-background'
                : 'bg-cinema-surfaceElevated text-cinema-text'
            }`}
          >
            {nextPicker
              ? isMyTurn
                ? 'É a sua vez de escolher!'
                : `É a vez de ${nextPicker.name}`
              : 'Aguardando o par entrar na sessão…'}
          </motion.div>
        </AnimatePresence>

        {session.currentMovie ? (
          <div className="w-full max-w-sm rounded-3xl bg-cinema-surface p-5 shadow-cozy sm:max-w-md">
            <p className="mb-1 text-xs font-medium uppercase tracking-wide text-cinema-textMuted">
              Filme escolhido
            </p>
            <h2 className="text-lg font-semibold text-cinema-text">{session.currentMovie.title}</h2>
            {session.currentMovie.coverUrl && (
              <img
                src={session.currentMovie.coverUrl}
                alt={session.currentMovie.title}
                className="mt-3 max-h-64 w-full rounded-2xl object-cover"
              />
            )}
            <button
              type="button"
              onClick={() => setIsMarkingWatched(true)}
              className="mt-4 h-11 w-full rounded-2xl bg-cinema-surfaceElevated px-4 text-sm font-semibold text-cinema-accent shadow-cozy transition hover:bg-cinema-primaryHover hover:text-cinema-text"
            >
              Marcar como assistido
            </button>
            <p className="mt-3 text-center text-xs text-cinema-textMuted">
              Marquem o filme como assistido para liberar a escolha do próximo.
            </p>
          </div>
        ) : (
          <p className="text-sm text-cinema-textMuted">Ainda não escolheram um filme por aqui.</p>
        )}
      </div>

      {canPickNext && (
        <div className="fixed inset-x-0 bottom-0 border-t border-white/10 bg-cinema-surface px-6 py-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
          <button
            type="button"
            onClick={() => setIsPicking(true)}
            className="mx-auto h-14 w-full max-w-2xl rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover"
          >
            Escolher filme
          </button>
        </div>
      )}

      {isPicking && <MoviePickForm onClose={() => setIsPicking(false)} />}

      {isMarkingWatched && session.currentMovie && (
        <MarkWatchedSheet
          sessionId={session.id}
          movieTitle={session.currentMovie.title}
          coverUrl={session.currentMovie.coverUrl}
          onClose={() => setIsMarkingWatched(false)}
        />
      )}
    </main>
  )
}
