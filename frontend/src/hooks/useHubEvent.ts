import { useEffect, useRef } from 'react'
import type { HubConnection } from '@microsoft/signalr'

export function useHubEvent<T>(
  connection: HubConnection | null,
  eventName: string,
  handler: (payload: T) => void,
) {
  const handlerRef = useRef(handler)

  useEffect(() => {
    handlerRef.current = handler
  })

  useEffect(() => {
    if (!connection) {
      return
    }

    const listener = (payload: T) => handlerRef.current(payload)
    connection.on(eventName, listener)

    return () => {
      connection.off(eventName, listener)
    }
  }, [connection, eventName])
}
