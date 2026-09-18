import { AnimatePresence, LayoutGroup, motion, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../../lib/motion'
import { useSession } from '../../../hooks/useSession'
import { VotingRoundProvider } from '../../../contexts/VotingRoundContext'
import { useVotingRound } from '../../../hooks/useVotingRound'
import { SelectionScreen } from './SelectionScreen'
import { BlindVotingScreen } from './BlindVotingScreen'
import { WinnerRevealScreen } from './WinnerRevealScreen'

function GrupoRoundRouter() {
  const { round, status } = useVotingRound()
  const shouldReduceMotion = useReducedMotion()

  if (status === 'loading') {
    return <p className="text-center text-sm text-cinema-textMuted">Carregando rodada…</p>
  }

  const phase = round?.status ?? 'Selecting'

  return (
    <AnimatePresence mode="popLayout" initial={false}>
      <motion.div
        key={phase}
        {...cozyMotion(shouldReduceMotion, {
          initial: { opacity: 0, y: 8 },
          animate: { opacity: 1, y: 0 },
          exit: { opacity: 0, y: -8 },
          transition: { duration: 0.3, ease: 'easeInOut' },
        })}
        className="flex flex-1 flex-col"
      >
        {phase === 'Voting' ? (
          <BlindVotingScreen />
        ) : phase === 'Completed' ? (
          <WinnerRevealScreen />
        ) : (
          <SelectionScreen />
        )}
      </motion.div>
    </AnimatePresence>
  )
}

interface GrupoSessionScreenProps {
  onOpenMovieLog: () => void
}

export function GrupoSessionScreen({ onOpenMovieLog }: GrupoSessionScreenProps) {
  const { session, switchSession } = useSession()

  if (!session) {
    return null
  }

  return (
    <VotingRoundProvider sessionId={session.id}>
      <LayoutGroup>
        <main className="mx-auto flex min-h-svh w-full max-w-2xl flex-col bg-cinema-background px-6 pb-32 pt-12">
          <header className="mb-8 flex flex-col items-center gap-1 text-center">
            <h1 className="font-heading text-2xl font-bold text-cinema-text">{session.title}</h1>
            <p className="text-sm text-cinema-textMuted">
              Código de convite:{' '}
              <span className="font-semibold tracking-[0.3em] text-cinema-accent">
                {session.inviteCode}
              </span>
            </p>
            <div className="mt-2 flex gap-4">
              <button
                type="button"
                onClick={onOpenMovieLog}
                className="rounded-lg px-2 py-1 text-xs font-medium text-cinema-textMuted transition hover:text-cinema-accent"
              >
                Histórico
              </button>
              <button
                type="button"
                onClick={switchSession}
                className="rounded-lg px-2 py-1 text-xs font-medium text-cinema-textMuted transition hover:text-cinema-accent"
              >
                Trocar de sessão
              </button>
            </div>
          </header>

          <GrupoRoundRouter />
        </main>
      </LayoutGroup>
    </VotingRoundProvider>
  )
}
