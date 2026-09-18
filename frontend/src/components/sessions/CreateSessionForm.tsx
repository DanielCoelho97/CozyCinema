import { useState, type FormEvent } from 'react'
import { useSession } from '../../hooks/useSession'
import type { SessionMode } from '../../types/session'

interface CreateSessionFormProps {
  onSwitchToJoin: () => void
}

export function CreateSessionForm({ onSwitchToJoin }: CreateSessionFormProps) {
  const { createSession, error } = useSession()
  const [title, setTitle] = useState('')
  const [mode, setMode] = useState<SessionMode>('Casal')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setIsSubmitting(true)
    try {
      await createSession({ title, mode })
    } catch {
      // erro já exposto via `error` do useSession
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="session-title" className="text-sm font-medium text-cinema-textMuted">
          Nome da sessão
        </label>
        <input
          id="session-title"
          type="text"
          required
          placeholder="Sexta à noite"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <span className="text-sm font-medium text-cinema-textMuted">Modo</span>
        <div className="flex gap-2 rounded-2xl bg-cinema-surface p-1">
          {(['Casal', 'Grupo'] as SessionMode[]).map((option) => (
            <button
              key={option}
              type="button"
              onClick={() => setMode(option)}
              className={`h-10 flex-1 rounded-xl text-sm font-semibold transition ${
                mode === option
                  ? 'bg-cinema-accent text-cinema-background'
                  : 'text-cinema-textMuted hover:text-cinema-text'
              }`}
            >
              {option}
            </button>
          ))}
        </div>
      </div>

      {error && <p className="text-sm text-cinema-danger">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="h-14 rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover disabled:opacity-60"
      >
        {isSubmitting ? 'Criando…' : 'Criar sessão'}
      </button>

      <button
        type="button"
        onClick={onSwitchToJoin}
        className="text-sm font-medium text-cinema-accent hover:text-cinema-accentHover"
      >
        Já tem um código? Entrar em uma sessão
      </button>
    </form>
  )
}
