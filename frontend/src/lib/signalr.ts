import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr'

export function createCinemaSessionConnection(apiUrl: string, sessionId: string, token: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(`${apiUrl}/hubs/cinema-session?sessionId=${sessionId}`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}
