export type RoundStatus = 'Selecting' | 'ReadyCheck' | 'Voting' | 'Completed'

export interface RoundMovie {
  id: string
  tmdbMovieId: number
  title: string
  coverUrl: string | null
  isSuggestedByCurrentUser: boolean
  suggestedByName: string | null
  voteCount: number | null
}

export interface VotingRound {
  id: string
  status: RoundStatus
  movies: RoundMovie[]
  readyCount: number
  totalActiveMembers: number
  isCurrentUserReady: boolean
  hasCurrentUserVoted: boolean
  winnerMovieId: string | null
}

export interface SuggestMoviePayload {
  tmdbMovieId: number
  title: string
  coverUrl?: string | null
}

export interface CastVotePayload {
  roundMovieId: string
}
