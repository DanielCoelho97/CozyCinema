namespace CozyCinema.Application.Auth.Exceptions;

public class EmailAlreadyInUseException(string email)
    : Exception($"O e-mail '{email}' já está em uso.");
