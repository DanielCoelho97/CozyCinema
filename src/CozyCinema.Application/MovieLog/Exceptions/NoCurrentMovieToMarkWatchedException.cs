namespace CozyCinema.Application.MovieLog.Exceptions;

public class NoCurrentMovieToMarkWatchedException()
    : Exception("Esta sessão não tem um filme selecionado no momento para marcar como assistido.");
