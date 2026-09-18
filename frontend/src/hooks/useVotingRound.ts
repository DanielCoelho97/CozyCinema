import { useContext } from 'react'
import { VotingRoundContext } from '../contexts/VotingRoundContext'

export function useVotingRound() {
  const context = useContext(VotingRoundContext)

  if (!context) {
    throw new Error('useVotingRound deve ser usado dentro de um VotingRoundProvider')
  }

  return context
}
