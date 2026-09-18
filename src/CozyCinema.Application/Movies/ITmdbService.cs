using CozyCinema.Application.Movies.Dtos;

namespace CozyCinema.Application.Movies;

public interface ITmdbService
{
    Task<IReadOnlyList<MovieSummaryDto>> SearchMoviesAsync(string query, int page, CancellationToken cancellationToken);

    Task<MovieDetailDto> GetMovieDetailAsync(int tmdbMovieId, CancellationToken cancellationToken);
}
