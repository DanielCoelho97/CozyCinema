import { useState } from 'react'
import { motion, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../lib/motion'
import { useMovieLog } from '../../hooks/useMovieLog'
import { LogMovieManualForm } from './LogMovieManualForm'
import type { MovieLogSortBy, MovieLogSortDirection, WatchedMovie } from '../../types/movieLog'

interface MovieLogScreenProps {
  sessionId: string
  onClose: () => void
}

function WatchedMovieCard({ movie }: { movie: WatchedMovie }) {
  const watchedDate = new Date(movie.watchedAt).toLocaleDateString('pt-BR')
  const shouldReduceMotion = useReducedMotion()

  return (
    <motion.div
      layout
      {...cozyMotion(shouldReduceMotion, {
        initial: { opacity: 0, y: 8 },
        animate: { opacity: 1, y: 0 },
        transition: { duration: 0.25, ease: 'easeInOut' },
      })}
      className="flex gap-3 rounded-3xl bg-cinema-surface p-4 shadow-cozy"
    >
      {movie.coverUrl ? (
        <img src={movie.coverUrl} alt={movie.title} className="h-36 w-24 flex-none rounded-2xl object-cover" />
      ) : (
        <div className="flex h-36 w-24 flex-none items-center justify-center rounded-2xl bg-cinema-surfaceElevated text-center text-[10px] text-cinema-textMuted">
          Sem capa
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col gap-1">
        <h3 className="truncate text-lg font-semibold text-cinema-text">{movie.title}</h3>

        <div className="flex flex-wrap items-center gap-2 text-xs text-cinema-textMuted">
          {movie.releaseYear && <span>{movie.releaseYear}</span>}
          <span className="rounded-2xl bg-cinema-accent/20 px-2 py-0.5 font-semibold text-cinema-accent">
            ★ {movie.rating.toFixed(1)}
          </span>
          {movie.director && <span>Dir. {movie.director}</span>}
        </div>

        {movie.genres.length > 0 && (
          <p className="text-xs text-cinema-textMuted">{movie.genres.join(', ')}</p>
        )}

        {movie.synopsis && <p className="line-clamp-2 text-sm text-cinema-textMuted">{movie.synopsis}</p>}

        <p className="mt-auto text-xs text-cinema-textMuted">
          Assistido em {watchedDate} · {movie.userName}
        </p>
      </div>
    </motion.div>
  )
}

export function MovieLogScreen({ sessionId, onClose }: MovieLogScreenProps) {
  const { movies, status, error, filters, setFilters, refresh } = useMovieLog({ sessionId })
  const [isLoggingManually, setIsLoggingManually] = useState(false)

  return (
    <main className="mx-auto flex min-h-svh w-full max-w-3xl flex-col bg-cinema-background px-6 pb-32 pt-12">
      <header className="mb-6 flex items-center justify-between">
        <button
          type="button"
          onClick={onClose}
          className="rounded-lg px-2 py-1 text-sm font-medium text-cinema-textMuted transition hover:text-cinema-accent"
        >
          ← Voltar
        </button>
        <h1 className="font-heading text-xl font-bold text-cinema-text">Movie Log</h1>
        <span className="w-10" />
      </header>

      <div className="mb-6 flex flex-col gap-3 rounded-3xl bg-cinema-surfaceElevated p-4 shadow-cozy">
        <div className="flex gap-2">
          <select
            value={filters.sortBy}
            onChange={(event) =>
              setFilters({ ...filters, sortBy: event.target.value as MovieLogSortBy })
            }
            className="h-11 flex-1 rounded-2xl bg-cinema-surface px-3 text-sm text-cinema-text outline-none ring-cinema-accent focus:ring-2"
          >
            <option value="watchedAt">Mais recentes</option>
            <option value="year">Ano</option>
            <option value="rating">Nota</option>
          </select>

          <select
            value={filters.sortDirection}
            onChange={(event) =>
              setFilters({ ...filters, sortDirection: event.target.value as MovieLogSortDirection })
            }
            className="h-11 rounded-2xl bg-cinema-surface px-3 text-sm text-cinema-text outline-none ring-cinema-accent focus:ring-2"
          >
            <option value="desc">↓</option>
            <option value="asc">↑</option>
          </select>
        </div>

        <div className="flex gap-2">
          <input
            type="text"
            value={filters.genre}
            onChange={(event) => setFilters({ ...filters, genre: event.target.value })}
            placeholder="Filtrar por gênero…"
            className="h-11 flex-1 rounded-2xl bg-cinema-surface px-3 text-sm text-cinema-text outline-none ring-cinema-accent focus:ring-2"
          />
          <input
            type="text"
            value={filters.director}
            onChange={(event) => setFilters({ ...filters, director: event.target.value })}
            placeholder="Filtrar por diretor…"
            className="h-11 flex-1 rounded-2xl bg-cinema-surface px-3 text-sm text-cinema-text outline-none ring-cinema-accent focus:ring-2"
          />
        </div>
      </div>

      {status === 'loading' && <p className="text-center text-sm text-cinema-textMuted">Carregando…</p>}
      {error && <p className="text-center text-sm text-cinema-danger">{error}</p>}

      {status === 'ready' && movies.length === 0 && (
        <p className="text-center text-sm text-cinema-textMuted">
          Nenhum filme assistido por aqui ainda.
        </p>
      )}

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        {movies.map((movie) => (
          <WatchedMovieCard key={movie.id} movie={movie} />
        ))}
      </div>

      <div className="fixed inset-x-0 bottom-0 border-t border-white/10 bg-cinema-surface px-6 py-4 pb-[max(1rem,env(safe-area-inset-bottom))]">
        <button
          type="button"
          onClick={() => setIsLoggingManually(true)}
          className="mx-auto h-14 w-full max-w-3xl rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover"
        >
          Adicionar filme ao histórico
        </button>
      </div>

      {isLoggingManually && (
        <LogMovieManualForm
          sessionId={sessionId}
          onClose={() => setIsLoggingManually(false)}
          onLogged={() => refresh().catch(() => {})}
        />
      )}
    </main>
  )
}
