import { useMovieSearch } from '../../hooks/useMovieSearch'
import type { MovieSummary } from '../../types/movie'

interface MovieSearchPickerProps {
  selected: MovieSummary | null
  onSelect: (movie: MovieSummary) => void
  onClear: () => void
}

export function MovieSearchPicker({ selected, onSelect, onClear }: MovieSearchPickerProps) {
  const { query, setQuery, results, isLoading, error } = useMovieSearch()

  if (selected) {
    return (
      <div className="flex items-center gap-3 rounded-2xl bg-cinema-surface p-3">
        {selected.coverUrl ? (
          <img
            src={selected.coverUrl}
            alt={selected.title}
            className="h-20 w-14 flex-none rounded-xl object-cover"
          />
        ) : (
          <div className="flex h-20 w-14 flex-none items-center justify-center rounded-xl bg-cinema-surfaceElevated text-center text-[10px] text-cinema-textMuted">
            Sem capa
          </div>
        )}
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold text-cinema-text">{selected.title}</p>
          {selected.releaseYear && <p className="text-xs text-cinema-textMuted">{selected.releaseYear}</p>}
        </div>
        <button
          type="button"
          onClick={onClear}
          className="flex-none text-xs font-medium text-cinema-textMuted hover:text-cinema-accent"
        >
          Trocar
        </button>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-3">
      <input
        type="search"
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        placeholder="Buscar filme na TMDB…"
        className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
      />

      {isLoading && <p className="text-center text-xs text-cinema-textMuted">Buscando…</p>}
      {error && <p className="text-center text-xs text-cinema-danger">{error}</p>}

      {results.length > 0 && (
        <ul className="flex max-h-72 flex-col gap-2 overflow-y-auto">
          {results.map((movie) => (
            <li key={movie.tmdbMovieId}>
              <button
                type="button"
                onClick={() => onSelect(movie)}
                className="flex w-full items-center gap-3 rounded-2xl bg-cinema-surface p-2 text-left transition hover:bg-cinema-surfaceElevated"
              >
                {movie.coverUrl ? (
                  <img
                    src={movie.coverUrl}
                    alt={movie.title}
                    className="h-16 w-11 flex-none rounded-lg object-cover"
                  />
                ) : (
                  <div className="flex h-16 w-11 flex-none items-center justify-center rounded-lg bg-cinema-surfaceElevated text-center text-[9px] text-cinema-textMuted">
                    Sem capa
                  </div>
                )}
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold text-cinema-text">{movie.title}</p>
                  <p className="text-xs text-cinema-textMuted">
                    {[movie.releaseYear, movie.rating ? `★ ${movie.rating.toFixed(1)}` : null]
                      .filter(Boolean)
                      .join(' · ')}
                  </p>
                </div>
              </button>
            </li>
          ))}
        </ul>
      )}

      {!isLoading && query.trim() && results.length === 0 && !error && (
        <p className="text-center text-xs text-cinema-textMuted">Nenhum filme encontrado.</p>
      )}
    </div>
  )
}
