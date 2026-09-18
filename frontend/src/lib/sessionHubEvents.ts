export const SessionHubEvent = {
  UserJoinedSession: 'UserJoinedSession',
  UserLeftSession: 'UserLeftSession',
  MovieSuggested: 'MovieSuggested',
  ReadyToVote: 'ReadyToVote',
  VotingStarted: 'VotingStarted',
  VoteSubmitted: 'VoteSubmitted',
  VotingCompleted: 'VotingCompleted',
  MovieSelected: 'MovieSelected',
  TurnPassed: 'TurnPassed',
  MovieWatched: 'MovieWatched',
  SessionError: 'SessionError',
} as const

export interface SessionErrorPayload {
  sessionId: string
  code: string
  message: string
}
