using CozyCinema.Application.MovieLog.Dtos;

namespace CozyCinema.Application.MovieLog;

public interface IMovieLogService
{
    Task<WatchedMovieDto> MarkCurrentMovieWatchedAsync(
        Guid userId,
        Guid sessionId,
        MarkCurrentMovieWatchedRequestDto request,
        CancellationToken cancellationToken);

    Task<WatchedMovieDto> LogWatchedMovieAsync(
        Guid userId,
        Guid sessionId,
        LogWatchedMovieRequestDto request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WatchedMovieDto>> GetSessionHistoryAsync(
        Guid userId,
        Guid sessionId,
        MovieLogFilter filter,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WatchedMovieDto>> GetMyHistoryAsync(
        Guid userId,
        MovieLogFilter filter,
        CancellationToken cancellationToken);
}
