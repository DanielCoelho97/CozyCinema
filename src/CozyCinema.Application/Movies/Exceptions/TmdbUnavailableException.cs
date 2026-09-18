namespace CozyCinema.Application.Movies.Exceptions;

public class TmdbUnavailableException(string message) : Exception(message);
