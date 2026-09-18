import { useContext } from 'react'
import { SessionContext } from '../contexts/SessionContext'

export function useSession() {
  const context = useContext(SessionContext)

  if (!context) {
    throw new Error('useSession deve ser usado dentro de um SessionProvider')
  }

  return context
}
