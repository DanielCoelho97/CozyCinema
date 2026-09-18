import type { Transition } from 'framer-motion'

// Ver docs/UI-DESIGN.md §6 — com `prefers-reduced-motion` ativo, toda animação vira um fade curto (~150ms) sem scale/movimento.
export const REDUCED_TRANSITION: Transition = { duration: 0.15, ease: 'easeInOut' }

interface CozyMotionProps {
  initial: Record<string, number | string>
  animate: Record<string, number | string>
  exit?: Record<string, number | string>
  transition: Transition
}

export function cozyMotion(
  shouldReduceMotion: boolean | null | undefined,
  full: CozyMotionProps,
): CozyMotionProps {
  if (!shouldReduceMotion) {
    return full
  }

  return {
    initial: { opacity: full.initial.opacity ?? 0 },
    animate: { opacity: full.animate.opacity ?? 1 },
    exit: full.exit ? { opacity: full.exit.opacity ?? 0 } : undefined,
    transition: REDUCED_TRANSITION,
  }
}
