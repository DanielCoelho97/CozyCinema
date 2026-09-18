import { useState } from 'react'
import { motion, useReducedMotion } from 'framer-motion'
import { useVotingRound } from '../../../hooks/useVotingRound'
import { useSession } from '../../../hooks/useSession'
import { MarkWatchedSheet } from '../../movielog/MarkWatchedSheet'

export function WinnerRevealScreen() {
  const { round } = useVotingRound()
  const { session } = useSession()
  const shouldReduceMotion = useReducedMotion()
  const [isMarkingWatched, setIsMarkingWatched] = useState(false)

  const winner = round?.movies.find((movie) => movie.id === round.winnerMovieId)

  if (!round || !winner || !session) {
    return null
  }

  const losers = round.movies.filter((movie) => movie.id !== winner.id)

  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-6 pb-32 text-center">
      <p className="text-sm font-medium uppercase tracking-wide text-cinema-textMuted">
        O filme escolhido é
      </p>

      <motion.div
        layoutId={`round-movie-${winner.id}`}
        transition={shouldReduceMotion ? { duration: 0.15 } : { duration: 0.7, ease: 'easeInOut' }}
        className="w-64 max-w-full overflow-hidden rounded-3xl bg-cinema-surface shadow-glowAccent ring-2 ring-cinema-accent"
      >
        {winner.coverUrl ? (
          <img src={winner.coverUrl} alt={winner.title} className="aspect-[2/3] w-full object-cover" />
        ) : (
          <div className="flex aspect-[2/3] w-full items-center justify-center bg-cinema-surfaceElevated px-4 text-center text-base text-cinema-text">
            {winner.title}
          </div>
        )}
      </motion.div>

      <h2 className="font-heading text-2xl font-bold text-cinema-text">{winner.title}</h2>

      {losers.length > 0 && (
        <motion.div
          initial="visible"
          animate="hidden"
          variants={{
            hidden: { transition: { staggerChildren: shouldReduceMotion ? 0 : 0.08 } },
          }}
          className="flex flex-wrap justify-center gap-3"
        >
          {losers.map((movie) => (
            <motion.div
              key={movie.id}
              variants={{
                visible: { opacity: 1, scale: 1 },
                hidden: shouldReduceMotion
                  ? { opacity: 0, transition: { duration: 0.15 } }
                  : { opacity: 0, scale: 0.9, transition: { duration: 0.3 } },
              }}
              className="h-24 w-16 overflow-hidden rounded-2xl bg-cinema-surface"
            >
              {movie.coverUrl && (
                <img src={movie.coverUrl} alt={movie.title} className="h-full w-full object-cover" />
              )}
            </motion.div>
          ))}
        </motion.div>
      )}

      {session.currentMovie?.tmdbMovieId === winner.tmdbMovieId ? (
        <button
          type="button"
          onClick={() => setIsMarkingWatched(true)}
          className="h-14 rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover"
        >
          Marcar como assistido
        </button>
      ) : (
        <p className="text-sm text-cinema-textMuted">
          Já assistido! Sugira um novo filme para começar outra rodada.
        </p>
      )}

      {isMarkingWatched && (
        <MarkWatchedSheet
          sessionId={session.id}
          movieTitle={winner.title}
          coverUrl={winner.coverUrl}
          onClose={() => setIsMarkingWatched(false)}
        />
      )}
    </div>
  )
}
