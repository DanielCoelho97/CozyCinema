import { useContext } from 'react'
import { RealtimeContext } from '../contexts/RealtimeContext'

export function useRealtime() {
  return useContext(RealtimeContext)
}
