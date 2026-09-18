import { useState } from 'react'
import { motion, AnimatePresence, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../../lib/motion'
import { useVotingRound } from '../../../hooks/useVotingRound'
import { MovieSearchPicker } from '../../movies/MovieSearchPicker'
import type { MovieSummary } from '../../../types/movie'

interface MovieSuggestFormProps {
  onClose: () => void
}

export function MovieSuggestForm({ onClose }: MovieSuggestFormProps) {
  const { suggestMovie, error } = useVotingRound()
  const [selected, setSelected] = useState<MovieSummary | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const shouldReduceMotion = useReducedMotion()

  const handleConfirm = async () => {
    if (!selected) {
      return
    }

    setIsSubmitting(true)
    try {
      await suggestMovie({
        tmdbMovieId: selected.tmdbMovieId,
        title: selected.title,
        coverUrl: selected.coverUrl,
      })
      setSelected(null)
      onClose()
    } catch {
      // erro já exposto via `error` do useVotingRound
    } finally {
      setIsSubmitting(false)
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
          Sugerir um filme para a rodada
        </h2>

        <div className="flex flex-col gap-4">
          <MovieSearchPicker
            selected={selected}
            onSelect={setSelected}
            onClear={() => setSelected(null)}
          />

          {error && <p className="text-sm text-cinema-danger">{error}</p>}

          <button
            type="button"
            onClick={handleConfirm}
            disabled={!selected || isSubmitting}
            className="h-14 rounded-2xl bg-cinema-accent px-6 text-base font-semibold text-cinema-background shadow-cozy transition hover:bg-cinema-accentHover disabled:opacity-60"
          >
            {isSubmitting ? 'Enviando…' : 'Sugerir filme'}
          </button>
        </div>
      </motion.div>
    </AnimatePresence>
  )
}
