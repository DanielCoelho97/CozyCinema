export type SessionMode = 'Casal' | 'Grupo'

export type MemberStatus = 'Active' | 'ReadyToVote' | 'Left'

export interface SessionMember {
  userId: string
  name: string
  status: MemberStatus
}

export interface CurrentMovie {
  tmdbMovieId: number
  title: string
  coverUrl: string | null
  pickedByUserId: string
}

export interface SessionSummary {
  id: string
  title: string
  iconUrl: string | null
  mode: SessionMode
  inviteCode: string
  activeMemberCount: number
}

export interface Session {
  id: string
  title: string
  iconUrl: string | null
  mode: SessionMode
  inviteCode: string
  lastPickerUserId: string | null
  nextPickerUserId: string | null
  currentMovie: CurrentMovie | null
  members: SessionMember[]
}

export interface CreateSessionPayload {
  title: string
  mode: SessionMode
  iconUrl?: string | null
}

export interface JoinSessionPayload {
  inviteCode: string
}

export interface SelectMoviePayload {
  tmdbMovieId: number
  title: string
  coverUrl?: string | null
}
