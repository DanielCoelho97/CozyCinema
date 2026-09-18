import { useState, type FormEvent } from 'react'
import { useAuth } from '../../hooks/useAuth'

interface RegisterFormProps {
  onSwitchToLogin: () => void
}

export function RegisterForm({ onSwitchToLogin }: RegisterFormProps) {
  const { register, error } = useAuth()
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setIsSubmitting(true)
    try {
      await register({ name, email, password })
    } catch {
      // erro já exposto via `error` do useAuth
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex w-full flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="register-name" className="text-sm font-medium text-cinema-textMuted">
          Nome
        </label>
        <input
          id="register-name"
          type="text"
          required
          autoComplete="name"
          value={name}
          onChange={(event) => setName(event.target.value)}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="register-email" className="text-sm font-medium text-cinema-textMuted">
          E-mail
        </label>
        <input
          id="register-email"
          type="email"
          required
          autoComplete="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="register-password" className="text-sm font-medium text-cinema-textMuted">
          Senha
        </label>
        <input
          id="register-password"
          type="password"
          required
          minLength={8}
          autoComplete="new-password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          className="h-12 rounded-2xl bg-cinema-surface px-4 text-base text-cinema-text outline-none ring-cinema-accent focus:ring-2"
        />
        <p className="text-xs text-cinema-textMuted">Mínimo de 8 caracteres.</p>
      </div>

      {error && <p className="text-sm text-cinema-danger">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="h-14 rounded-2xl bg-cinema-primary px-6 text-base font-semibold text-cinema-text shadow-cozy transition hover:bg-cinema-primaryHover disabled:opacity-60"
      >
        {isSubmitting ? 'Criando conta…' : 'Criar conta'}
      </button>

      <button
        type="button"
        onClick={onSwitchToLogin}
        className="text-sm font-medium text-cinema-accent hover:text-cinema-accentHover"
      >
        Já tem conta? Entrar
      </button>
    </form>
  )
}
