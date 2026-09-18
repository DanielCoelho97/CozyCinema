import { useState } from 'react'
import { useVotingRound } from '../../../hooks/useVotingRound'
import { MovieVoteCard } from './MovieVoteCard'

export function BlindVotingScreen() {
  const { round, castVote, error } = useVotingRound()
  const [selectedMovieId, setSelectedMovieId] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const movies = round?.movies ?? []
  const hasVoted = round?.hasCurrentUserVoted ?? false

  const handleConfirm = async () => {
    if (!selectedMovieId) {
      return
    }

    setIsSubmitting(true)
    try {
      await castVote({ roundMovieId: selectedMovieId })
    } catch {
      // erro já exposto via `error` do useVotingRound
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex flex-1 flex-col gap-6 pb-32">
      <div className="text-center">
        <h2 className="font-heading text-lg font-semibold text-cinema-text">Votação cega</h2>
        <p className="mt-1 text-sm text-cinema-textMuted">
          Escolha um filme sem saber quem sugeriu cada um. Você não pode votar no filme que você mesmo
          sugeriu.
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
        {movies.map((movie) => (
          <MovieVoteCard
            key={movie.id}
            movie={movie}
            selected={selectedMovieId === movie.id}
            disabled={hasVoted || movie.isSuggestedByCurrentUser}
            onSelect={() => setSelectedMovieId(movie.id)}
          />
        ))}
      </div>

      {error && <p className="text-center text-sm text-cinema-danger">{error}</p>}

      {hasVoted && (
        <p className="text-center text-sm text-cinema-textMuted">
          Voto registrado! Aguardando os outros votarem…
        </p>
      )}

      {!hasVoted && (
        <div className="fixed inset-x-0 bottom-0 border-t border-white/10 bg-cinema-surface px-6 py-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
          <button
            type="button"
            onClick={handleConfirm}
            disabled={!selectedMovieId || isSubmitting}
            className="mx-auto h-14 w-full max-w-2xl rounded-2xl bg-cinema-accent px-6 text-base font-semibold text-cinema-background shadow-cozy transition hover:bg-cinema-accentHover disabled:opacity-60"
          >
            {isSubmitting ? 'Confirmando…' : 'Confirmar Voto'}
          </button>
        </div>
      )}
    </div>
  )
}
