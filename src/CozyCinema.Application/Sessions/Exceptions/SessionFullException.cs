namespace CozyCinema.Application.Sessions.Exceptions;

public class SessionFullException()
    : Exception("Esta sessão já atingiu o número máximo de membros.");
