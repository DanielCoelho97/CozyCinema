import { useEffect, useState } from 'react'

type HealthStatus = 'loading' | 'online' | 'offline'

const API_URL = import.meta.env.VITE_API_URL as string

export function useHealthCheck() {
  const [status, setStatus] = useState<HealthStatus>('loading')

  useEffect(() => {
    const controller = new AbortController()

    fetch(`${API_URL}/api/health`, { signal: controller.signal })
      .then((response) => setStatus(response.ok ? 'online' : 'offline'))
      .catch(() => setStatus('offline'))

    return () => controller.abort()
  }, [])

  return status
}
