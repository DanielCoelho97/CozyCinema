import { useState, type FormEvent } from 'react'
import { useAuth } from '../../hooks/useAuth'

interface LoginFormProps {
  onSwitchToRegister: () => void
}

export function LoginForm({ onSwitchToRegister }: LoginFormProps) {
  const { login, error } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setIsSubmitting(true)
    try {
      await login({ email, password })
    } catch {
      // erro já exposto via `error` do useAuth
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="login-email" className="text-sm font-medium text-cinema-textMuted">
          E-mail
        </label>
        <input
          id="login-email"
          type="email"
          required
          autoComplete="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="login-password" className="text-sm font-medium text-cinema-textMuted">
          Senha
        </label>
        <input
          id="login-password"
          type="password"
          required
          autoComplete="current-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
      </div>

      {error && <p className="text-sm text-cinema-danger">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="h-14 rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover disabled:opacity-60"
      >
        {isSubmitting ? 'Entrando…' : 'Entrar'}
      </button>

      <button
        type="button"
        onClick={onSwitchToRegister}
        className="text-sm font-medium text-cinema-accent hover:text-cinema-accentHover"
      >
        Não tem conta? Criar conta
      </button>
    </form>
  )
}
