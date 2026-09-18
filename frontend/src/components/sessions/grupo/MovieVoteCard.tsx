import { motion } from 'framer-motion'
import type { RoundMovie } from '../../../types/voting'

interface MovieVoteCardProps {
  movie: RoundMovie
  selected: boolean
  disabled: boolean
  onSelect: () => void
}

export function MovieVoteCard({ movie, selected, disabled, onSelect }: MovieVoteCardProps) {
  return (
    <motion.button
      type="button"
      layoutId={`round-movie-${movie.id}`}
      onClick={onSelect}
      disabled={disabled}
      className={`relative aspect-[2/3] w-full overflow-hidden rounded-3xl bg-cinema-surface text-left shadow-cozy transition disabled:cursor-not-allowed ${
        movie.isSuggestedByCurrentUser ? 'grayscale' : ''
      } ${disabled ? 'opacity-50' : ''} ${selected ? 'ring-2 ring-cinema-accent' : ''}`}
    >
      {movie.coverUrl ? (
        <img src={movie.coverUrl} alt={movie.title} className="h-full w-full object-cover" />
      ) : (
        <div className="flex h-full w-full items-center justify-center bg-cinema-surfaceElevated px-4 text-center text-sm text-cinema-textMuted">
          {movie.title}
        </div>
      )}

      {movie.isSuggestedByCurrentUser && (
        <div className="absolute right-2 top-2 rounded-full bg-cinema-accent px-2.5 py-1 text-xs font-bold text-cinema-background shadow-cozy">
          Sua sugestão
        </div>
      )}

      <div className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/80 via-black/30 to-transparent p-4 pt-10">
        <p className="text-sm font-semibold text-cinema-text">{movie.title}</p>
      </div>
    </motion.button>
  )
}
