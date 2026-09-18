namespace CozyCinema.Application.Sessions.Exceptions;

public class SessionNotReadyException()
    : Exception("A sessão precisa de 2 membros ativos para escolher um filme.");
