using CozyCinema.Application.MovieLog;
using CozyCinema.Application.MovieLog.Dtos;
using CozyCinema.Application.MovieLog.Exceptions;
using CozyCinema.Application.Movies;
using CozyCinema.Application.RealTime;
using CozyCinema.Application.RealTime.Events;
using CozyCinema.Application.Sessions.Exceptions;
using CozyCinema.Domain.Entities;
using CozyCinema.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CozyCinema.Infrastructure.MovieLog;

public class MovieLogService(CozyCinemaDbContext dbContext, ITmdbService tmdbService, ISessionEventPublisher eventPublisher)
    : IMovieLogService
{
    public async Task<WatchedMovieDto> MarkCurrentMovieWatchedAsync(
        Guid userId,
        Guid sessionId,
        MarkCurrentMovieWatchedRequestDto request,
        CancellationToken cancellationToken)
    {
        var session = await LoadSessionForMemberAsync(userId, sessionId, cancellationToken);

        if (session.CurrentMovieTmdbId is null)
        {
            throw new NoCurrentMovieToMarkWatchedException();
        }

        var detail = await tmdbService.GetMovieDetailAsync(session.CurrentMovieTmdbId.Value, cancellationToken);
        var user = session.Members.Single(m => m.UserId == userId).User;

        var watchedMovie = BuildWatchedMovie(session, user, detail, request.WatchedAt);

        session.CurrentMovieTmdbId = null;
        session.CurrentMovieTitle = null;
        session.CurrentMovieCoverUrl = null;

        dbContext.WatchedMovies.Add(watchedMovie);
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.MovieWatchedAsync(
            new MovieWatchedEvent(session.Id, watchedMovie.Id, watchedMovie.TmdbMovieId, watchedMovie.Title, userId),
            cancellationToken);

        return ToDto(watchedMovie, session.Title, user.Name);
    }

    public async Task<WatchedMovieDto> LogWatchedMovieAsync(
        Guid userId,
        Guid sessionId,
        LogWatchedMovieRequestDto request,
        CancellationToken cancellationToken)
    {
        var session = await LoadSessionForMemberAsync(userId, sessionId, cancellationToken);
        var detail = await tmdbService.GetMovieDetailAsync(request.TmdbMovieId, cancellationToken);
        var user = session.Members.Single(m => m.UserId == userId).User;

        var watchedMovie = BuildWatchedMovie(session, user, detail, request.WatchedAt);

        dbContext.WatchedMovies.Add(watchedMovie);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(watchedMovie, session.Title, user.Name);
    }

    public async Task<IReadOnlyList<WatchedMovieDto>> GetSessionHistoryAsync(
        Guid userId,
        Guid sessionId,
        MovieLogFilter filter,
        CancellationToken cancellationToken)
    {
        var session = await LoadSessionForMemberAsync(userId, sessionId, cancellationToken);

        var watchedMovies = await dbContext.WatchedMovies
            .AsNoTracking()
            .Include(wm => wm.User)
            .Where(wm => wm.SessionId == sessionId)
            .ToListAsync(cancellationToken);

        return ApplyFilterAndSort(watchedMovies, filter)
            .Select(wm => ToDto(wm, session.Title, wm.User.Name))
            .ToList();
    }

    public async Task<IReadOnlyList<WatchedMovieDto>> GetMyHistoryAsync(
        Guid userId,
        MovieLogFilter filter,
        CancellationToken cancellationToken)
    {
        var watchedMovies = await dbContext.WatchedMovies
            .AsNoTracking()
            .Include(wm => wm.User)
            .Include(wm => wm.Session)
            .Where(wm => wm.UserId == userId)
            .ToListAsync(cancellationToken);

        return ApplyFilterAndSort(watchedMovies, filter)
            .Select(wm => ToDto(wm, wm.Session.Title, wm.User.Name))
            .ToList();
    }

    private static IEnumerable<WatchedMovie> ApplyFilterAndSort(IEnumerable<WatchedMovie> watchedMovies, MovieLogFilter filter)
    {
        var query = watchedMovies;

        if (!string.IsNullOrWhiteSpace(filter.Genre))
        {
            query = query.Where(wm => SplitGenres(wm.Genres).Contains(filter.Genre, StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Director))
        {
            query = query.Where(wm => wm.Director is not null
                && wm.Director.Contains(filter.Director, StringComparison.OrdinalIgnoreCase));
        }

        var descending = !string.Equals(filter.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        return filter.SortBy?.ToLowerInvariant() switch
        {
            "year" => descending ? query.OrderByDescending(wm => wm.ReleaseYear) : query.OrderBy(wm => wm.ReleaseYear),
            "rating" => descending ? query.OrderByDescending(wm => wm.Rating) : query.OrderBy(wm => wm.Rating),
            _ => query.OrderByDescending(wm => wm.WatchedAt),
        };
    }

    private static WatchedMovie BuildWatchedMovie(
        Session session,
        User user,
        Application.Movies.Dtos.MovieDetailDto detail,
        DateTime? watchedAt) => new()
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Session = session,
            UserId = user.Id,
            User = user,
            TmdbMovieId = detail.TmdbMovieId,
            Title = detail.Title,
            CoverUrl = detail.CoverUrl,
            Synopsis = detail.Synopsis,
            Rating = detail.Rating ?? 0,
            Genres = string.Join(',', detail.Genres),
            Director = detail.Director,
            ReleaseYear = detail.ReleaseDate?.Year,
            WatchedAt = watchedAt ?? DateTime.UtcNow,
        };

    private async Task<Session> LoadSessionForMemberAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .Include(s => s.Members)
            .ThenInclude(m => m.User)
            .SingleOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        if (session is null)
        {
            throw new SessionNotFoundException();
        }

        var isMember = session.Members.Any(m => m.UserId == userId && m.Status != MemberStatus.Left);

        if (!isMember)
        {
            throw new NotSessionMemberException();
        }

        return session;
    }

    private static List<string> SplitGenres(string genres) =>
        genres.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static WatchedMovieDto ToDto(WatchedMovie watchedMovie, string sessionTitle, string userName) => new()
    {
        Id = watchedMovie.Id,
        SessionId = watchedMovie.SessionId,
        SessionTitle = sessionTitle,
        UserId = watchedMovie.UserId,
        UserName = userName,
        TmdbMovieId = watchedMovie.TmdbMovieId,
        Title = watchedMovie.Title,
        CoverUrl = watchedMovie.CoverUrl,
        Synopsis = watchedMovie.Synopsis,
        Rating = watchedMovie.Rating,
        Genres = SplitGenres(watchedMovie.Genres),
        Director = watchedMovie.Director,
        ReleaseYear = watchedMovie.ReleaseYear,
        WatchedAt = watchedMovie.WatchedAt,
    };
}
