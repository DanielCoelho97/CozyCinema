import { useState } from 'react'
import { motion, AnimatePresence, useReducedMotion } from 'framer-motion'
import { cozyMotion } from '../../lib/motion'
import { LoginForm } from './LoginForm'
import { RegisterForm } from './RegisterForm'

export function AuthScreen() {
  const [mode, setMode] = useState<'login' | 'register'>('login')
  const shouldReduceMotion = useReducedMotion()

  return (
    <main className="flex min-h-svh flex-col items-center justify-center gap-8 bg-cinema-background px-6 py-12">
      <motion.h1
        {...cozyMotion(shouldReduceMotion, {
          initial: { opacity: 0, y: -8 },
          animate: { opacity: 1, y: 0 },
          transition: { duration: 0.35, ease: 'easeInOut' },
        })}
        className="font-heading text-2xl font-bold text-cinema-text"
      >
        🍿 Cozy Cinema
      </motion.h1>

      <div className="w-full max-w-sm rounded-3xl bg-cinema-surfaceElevated p-6 shadow-cozyLg sm:max-w-md">
        <AnimatePresence mode="wait">
          {mode === 'login' ? (
            <motion.div
              key="login"
              {...cozyMotion(shouldReduceMotion, {
                initial: { opacity: 0, x: -8 },
                animate: { opacity: 1, x: 0 },
                exit: { opacity: 0, x: 8 },
                transition: { duration: 0.25, ease: 'easeInOut' },
              })}
            >
              <LoginForm onSwitchToRegister={() => setMode('register')} />
            </motion.div>
          ) : (
            <motion.div
              key="register"
              {...cozyMotion(shouldReduceMotion, {
                initial: { opacity: 0, x: 8 },
                animate: { opacity: 1, x: 0 },
                exit: { opacity: 0, x: -8 },
                transition: { duration: 0.25, ease: 'easeInOut' },
              })}
            >
              <RegisterForm onSwitchToLogin={() => setMode('login')} />
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </main>
  )
}
