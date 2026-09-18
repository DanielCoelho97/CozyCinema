import { useState } from 'react'
import { motion, AnimatePresence, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../lib/motion'
import { useMovieLogActions } from '../../hooks/useMovieLogActions'

interface MarkWatchedSheetProps {
  sessionId: string
  movieTitle: string
  coverUrl: string | null
  onClose: () => void
}

export function MarkWatchedSheet({ sessionId, movieTitle, coverUrl, onClose }: MarkWatchedSheetProps) {
  const { markCurrentMovieWatched, isSubmitting, error } = useMovieLogActions(sessionId)
  const [isDone, setIsDone] = useState(false)
  const shouldReduceMotion = useReducedMotion()

  const handleConfirm = async () => {
    try {
      await markCurrentMovieWatched()
      setIsDone(true)
      setTimeout(onClose, 900)
    } catch {
      // erro já exposto via `error`
    }
  }

  return (
    <AnimatePresence>
      <motion.div
        key="backdrop"
        {...cozyMotion(shouldReduceMotion, {
          initial: { opacity: 0 },
          animate: { opacity: 1 },
          exit: { opacity: 0 },
          transition: { duration: 0.25, ease: 'easeInOut' },
        })}
        className="fixed inset-0 z-40 bg-black/60"
        onClick={onClose}
      />
      <motion.div
        key="sheet"
        {...cozyMotion(shouldReduceMotion, {
          initial: { y: '100%' },
          animate: { y: 0 },
          exit: { y: '100%' },
          transition: { duration: 0.3, ease: 'easeInOut' },
        })}
        className="fixed inset-x-0 bottom-0 z-50 mx-auto max-h-[85svh] w-full max-w-2xl overflow-y-auto rounded-t-3xl bg-cinema-surfaceElevated p-6 pb-[max(1.5rem,env(safe-area-inset-bottom))] shadow-cozyLg"
      >
        <div className="mx-auto mb-4 h-1.5 w-12 rounded-full bg-cinema-textMuted/40" />

        <h2 className="mb-4 font-heading text-lg font-semibold text-cinema-text">
          {isDone ? 'Adicionado ao histórico!' : 'Marcar como assistido?'}
        </h2>

        <div className="flex items-center gap-3 rounded-2xl bg-cinema-surface p-3">
          {coverUrl ? (
            <img src={coverUrl} alt={movieTitle} className="h-20 w-14 flex-none rounded-xl object-cover" />
          ) : (
            <div className="flex h-20 w-14 flex-none items-center justify-center rounded-xl bg-cinema-surfaceElevated text-center text-[10px] text-cinema-textMuted">
              Sem capa
            </div>
          )}
          <p className="min-w-0 flex-1 truncate text-sm font-semibold text-cinema-text">{movieTitle}</p>
        </div>

        {error && <p className="mt-4 text-sm text-cinema-danger">{error}</p>}

        {!isDone && (
          <button
            type="button"
            onClick={handleConfirm}
            disabled={isSubmitting}
            className="mt-4 h-14 w-full rounded-2xl bg-cinema-accent px-6 text-base font-semibold text-cinema-background shadow-cozy transition hover:bg-cinema-accentHover disabled:opacity-60"
          >
            {isSubmitting ? 'Salvando…' : 'Assistido!'}
          </button>
        )}
      </motion.div>
    </AnimatePresence>
  )
}
