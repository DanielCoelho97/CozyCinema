export interface WatchedMovie {
  id: string
  sessionId: string
  sessionTitle: string
  userId: string
  userName: string
  tmdbMovieId: number
  title: string
  coverUrl: string | null
  synopsis: string | null
  rating: number
  genres: string[]
  director: string | null
  releaseYear: number | null
  watchedAt: string
}

export type MovieLogSortBy = 'watchedAt' | 'year' | 'rating'
export type MovieLogSortDirection = 'asc' | 'desc'

export interface MovieLogFilters {
  sortBy: MovieLogSortBy
  sortDirection: MovieLogSortDirection
  genre: string
  director: string
}

export interface LogWatchedMoviePayload {
  tmdbMovieId: number
  watchedAt?: string | null
}
