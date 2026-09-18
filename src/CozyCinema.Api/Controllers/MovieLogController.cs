using CozyCinema.Api.Extensions;
using CozyCinema.Application.MovieLog;
using CozyCinema.Application.MovieLog.Dtos;
using CozyCinema.Application.MovieLog.Exceptions;
using CozyCinema.Application.Movies.Exceptions;
using CozyCinema.Application.Sessions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyCinema.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/movie-log")]
public class MovieLogController(IMovieLogService movieLogService) : ControllerBase
{
    [HttpPost("{sessionId:guid}/from-current")]
    public async Task<ActionResult<WatchedMovieDto>> MarkCurrentMovieWatched(
        Guid sessionId,
        MarkCurrentMovieWatchedRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var watchedMovie = await movieLogService.MarkCurrentMovieWatchedAsync(userId, sessionId, request, cancellationToken);
            return Ok(watchedMovie);
        }
        catch (SessionNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotSessionMemberException)
        {
            return Forbid();
        }
        catch (NoCurrentMovieToMarkWatchedException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (MovieNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (TmdbUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpPost("{sessionId:guid}/manual")]
    public async Task<ActionResult<WatchedMovieDto>> LogWatchedMovie(
        Guid sessionId,
        LogWatchedMovieRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var watchedMovie = await movieLogService.LogWatchedMovieAsync(userId, sessionId, request, cancellationToken);
            return Ok(watchedMovie);
        }
        catch (SessionNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotSessionMemberException)
        {
            return Forbid();
        }
        catch (MovieNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (TmdbUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<ActionResult<IReadOnlyList<WatchedMovieDto>>> GetSessionHistory(
        Guid sessionId,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] string? genre,
        [FromQuery] string? director,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var history = await movieLogService.GetSessionHistoryAsync(
                userId,
                sessionId,
                new MovieLogFilter(sortBy, sortDirection, genre, director),
                cancellationToken);
            return Ok(history);
        }
        catch (SessionNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotSessionMemberException)
        {
            return Forbid();
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<IReadOnlyList<WatchedMovieDto>>> GetMyHistory(
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDirection,
        [FromQuery] string? genre,
        [FromQuery] string? director,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var history = await movieLogService.GetMyHistoryAsync(
            userId,
            new MovieLogFilter(sortBy, sortDirection, genre, director),
            cancellationToken);
        return Ok(history);
    }
}
