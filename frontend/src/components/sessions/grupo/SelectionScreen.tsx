import { useState } from 'react'
import { useVotingRound } from '../../../hooks/useVotingRound'
import { MovieSuggestForm } from './MovieSuggestForm'

export function SelectionScreen() {
  const { round, markReady, error } = useVotingRound()
  const [isSuggesting, setIsSuggesting] = useState(false)
  const [isMarkingReady, setIsMarkingReady] = useState(false)

  const movies = round?.movies ?? []
  const hasMovies = movies.length > 0
  const isReady = round?.isCurrentUserReady ?? false
  const hasSuggested = movies.some((movie) => movie.isSuggestedByCurrentUser)

  const handleMarkReady = async () => {
    setIsMarkingReady(true)
    try {
      await markReady()
    } catch {
      // erro já exposto via `error` do useVotingRound
    } finally {
      setIsMarkingReady(false)
    }
  }

  return (
    <div className="flex flex-1 flex-col gap-6 pb-32">
      <div className="text-center">
        <p className="text-sm text-cinema-textMuted">
          {round ? `${round.readyCount} de ${round.totalActiveMembers} prontos` : 'Carregando…'}
        </p>
      </div>

      {hasMovies ? (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
          {movies.map((movie) => (
            <div
              key={movie.id}
              className="overflow-hidden rounded-3xl bg-cinema-surface shadow-cozy"
            >
              {movie.coverUrl ? (
                <img src={movie.coverUrl} alt={movie.title} className="h-40 w-full object-cover" />
              ) : (
                <div className="flex h-40 w-full items-center justify-center bg-cinema-surfaceElevated px-3 text-center text-sm text-cinema-textMuted">
                  {movie.title}
                </div>
              )}
              <div className="p-3">
                <p className="text-sm font-semibold text-cinema-text">{movie.title}</p>
                {movie.suggestedByName && (
                  <p className="mt-1 text-xs text-cinema-textMuted">
                    sugerido por {movie.suggestedByName}
                  </p>
                )}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-center text-sm text-cinema-textMuted">
          Ainda não há filmes sugeridos para esta rodada.
        </p>
      )}

      {error && <p className="text-center text-sm text-cinema-danger">{error}</p>}

      {isReady ? (
        <p className="text-center text-sm text-cinema-textMuted">Aguardando os outros…</p>
      ) : (
        hasMovies &&
        !hasSuggested && (
          <button
            type="button"
            onClick={() => setIsSuggesting(true)}
            className="text-center text-xs font-medium text-cinema-textMuted hover:text-cinema-accent"
          >
            Sugerir outro filme
          </button>
        )
      )}

      {!isReady && (
        <div className="fixed inset-x-0 bottom-0 border-t border-white/10 bg-cinema-surface px-6 py-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
          {hasMovies ? (
            <button
              type="button"
              onClick={handleMarkReady}
              disabled={isMarkingReady}
              className="mx-auto h-14 w-full max-w-2xl rounded-2xl bg-cinema-accent px-6 text-base font-semibold text-cinema-background shadow-cozy transition hover:bg-cinema-accentHover disabled:opacity-60"
            >
              {isMarkingReady ? 'Confirmando…' : 'Pronto para Votar'}
            </button>
          ) : (
            <button
              type="button"
              onClick={() => setIsSuggesting(true)}
              className="mx-auto h-14 w-full max-w-2xl rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover"
            >
              Sugerir Filme
            </button>
          )}
        </div>
      )}

      {isSuggesting && <MovieSuggestForm onClose={() => setIsSuggesting(false)} />}
    </div>
  )
}
