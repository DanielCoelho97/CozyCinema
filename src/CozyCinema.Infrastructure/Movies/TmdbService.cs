using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CozyCinema.Application.Movies;
using CozyCinema.Application.Movies.Dtos;
using CozyCinema.Application.Movies.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CozyCinema.Infrastructure.Movies;

public class TmdbService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IOptions<TmdbOptions> options,
    ILogger<TmdbService> logger) : ITmdbService
{
    private const string TmdbClientName = "TmdbClient";
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly TmdbOptions _options = options.Value;

    public async Task<IReadOnlyList<MovieSummaryDto>> SearchMoviesAsync(string query, int page, CancellationToken cancellationToken)
    {
        var cacheKey = $"tmdb:search:{query}:{page}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<MovieSummaryDto>? cached) && cached is not null)
        {
            return cached;
        }

        var client = httpClientFactory.CreateClient(TmdbClientName);
        var url = $"search/movie?api_key={Uri.EscapeDataString(_options.ApiKey)}&query={Uri.EscapeDataString(query)}&page={page}&language=pt-BR&include_adult=false";

        TmdbSearchResponse? response;
        try
        {
            response = await client.GetFromJsonAsync<TmdbSearchResponse>(url, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(ex, "Falha ao buscar filmes na TMDB para a query '{Query}'", query);
            throw new TmdbUnavailableException("Não foi possível buscar filmes na TMDB no momento.");
        }

        var results = (response?.Results ?? [])
            .Select(MapToSummary)
            .ToList();

        cache.Set(cacheKey, (IReadOnlyList<MovieSummaryDto>)results, CacheDuration);

        return results;
    }

    public async Task<MovieDetailDto> GetMovieDetailAsync(int tmdbMovieId, CancellationToken cancellationToken)
    {
        var cacheKey = $"tmdb:movie:{tmdbMovieId}";

        if (cache.TryGetValue(cacheKey, out MovieDetailDto? cached) && cached is not null)
        {
            return cached;
        }

        var client = httpClientFactory.CreateClient(TmdbClientName);
        var url = $"movie/{tmdbMovieId}?api_key={Uri.EscapeDataString(_options.ApiKey)}&language=pt-BR&append_to_response=credits";

        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await client.GetAsync(url, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Falha ao buscar detalhe do filme TMDB {TmdbMovieId}", tmdbMovieId);
            throw new TmdbUnavailableException("Não foi possível buscar os detalhes do filme na TMDB no momento.");
        }

        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
        {
            throw new MovieNotFoundException(tmdbMovieId);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("TMDB retornou {StatusCode} para o filme {TmdbMovieId}", httpResponse.StatusCode, tmdbMovieId);
            throw new TmdbUnavailableException("Não foi possível buscar os detalhes do filme na TMDB no momento.");
        }

        TmdbMovieDetail? detail;
        try
        {
            detail = await httpResponse.Content.ReadFromJsonAsync<TmdbMovieDetail>(cancellationToken);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Resposta inesperada da TMDB para o filme {TmdbMovieId}", tmdbMovieId);
            throw new TmdbUnavailableException("Não foi possível buscar os detalhes do filme na TMDB no momento.");
        }

        if (detail is null)
        {
            throw new MovieNotFoundException(tmdbMovieId);
        }

        var mapped = MapToDetail(detail);
        cache.Set(cacheKey, mapped, CacheDuration);

        return mapped;
    }

    private static MovieSummaryDto MapToSummary(TmdbMovieSummary movie) => new()
    {
        TmdbMovieId = movie.Id,
        Title = movie.Title,
        CoverUrl = BuildCoverUrl(movie.PosterPath),
        ReleaseYear = ParseYear(movie.ReleaseDate),
        Rating = movie.VoteAverage,
    };

    private static MovieDetailDto MapToDetail(TmdbMovieDetail movie) => new()
    {
        TmdbMovieId = movie.Id,
        Title = movie.Title,
        CoverUrl = BuildCoverUrl(movie.PosterPath),
        Synopsis = movie.Overview,
        Rating = movie.VoteAverage,
        Genres = movie.Genres.Select(genre => genre.Name).ToList(),
        Director = movie.Credits?.Crew.FirstOrDefault(member => member.Job == "Director")?.Name,
        ReleaseDate = ParseDate(movie.ReleaseDate),
    };

    private static string? BuildCoverUrl(string? posterPath) =>
        string.IsNullOrEmpty(posterPath) ? null : $"{ImageBaseUrl}{posterPath}";

    private static int? ParseYear(string? releaseDate) =>
        DateOnly.TryParse(releaseDate, out var date) ? date.Year : null;

    private static DateOnly? ParseDate(string? releaseDate) =>
        DateOnly.TryParse(releaseDate, out var date) ? date : null;
}
