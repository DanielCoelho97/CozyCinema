namespace CozyCinema.Application.Movies.Exceptions;

public class MovieNotFoundException(int tmdbMovieId) : Exception($"Filme TMDB {tmdbMovieId} não encontrado.");
