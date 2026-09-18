import { useState, type FormEvent } from 'react'
import { useSession } from '../../hooks/useSession'

interface JoinSessionFormProps {
  onSwitchToCreate: () => void
}

export function JoinSessionForm({ onSwitchToCreate }: JoinSessionFormProps) {
  const { joinSession, error } = useSession()
  const [inviteCode, setInviteCode] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setIsSubmitting(true)
    try {
      await joinSession({ inviteCode: inviteCode.trim().toUpperCase() })
    } catch {
      // erro já exposto via `error` do useSession
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="invite-code" className="text-sm font-medium text-cinema-textMuted">
          Código de convite
        </label>
        <input
          id="invite-code"
          type="text"
          required
          autoCapitalize="characters"
          placeholder="Ex: 7K3QXZ"
          value={inviteCode}
          onChange={(event) => setInviteCode(event.target.value.toUpperCase())}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-center text-lg font-semibold tracking-[0.3em] text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
      </div>

      {error && <p className="text-sm text-cinema-danger">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="h-14 rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover disabled:opacity-60"
      >
        {isSubmitting ? 'Entrando…' : 'Entrar na sessão'}
      </button>

      <button
        type="button"
        onClick={onSwitchToCreate}
        className="text-sm font-medium text-cinema-accent hover:text-cinema-accentHover"
      >
        Quer criar uma sessão nova?
      </button>
    </form>
  )
}
