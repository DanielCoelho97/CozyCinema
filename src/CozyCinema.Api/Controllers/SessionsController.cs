using CozyCinema.Api.Extensions;
using CozyCinema.Application.Sessions;
using CozyCinema.Application.Sessions.Dtos;
using CozyCinema.Application.Sessions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyCinema.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions")]
public class SessionsController(ISessionService sessionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SessionDto>> CreateSession(CreateSessionRequestDto request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var session = await sessionService.CreateSessionAsync(userId, request, cancellationToken);
        return Ok(session);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionSummaryDto>>> GetMySessions(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var sessions = await sessionService.GetMySessionsAsync(userId, cancellationToken);
        return Ok(sessions);
    }

    [HttpPost("join")]
    public async Task<ActionResult<SessionDto>> JoinSession(JoinSessionRequestDto request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var session = await sessionService.JoinSessionAsync(userId, request, cancellationToken);
            return Ok(session);
        }
        catch (SessionNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (SessionFullException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<SessionDto>> GetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var session = await sessionService.GetSessionAsync(userId, sessionId, cancellationToken);
            return Ok(session);
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

    [HttpPost("{sessionId:guid}/casal/movie-pick")]
    public async Task<ActionResult<SessionDto>> SelectMovie(Guid sessionId, SelectMovieRequestDto request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var session = await sessionService.SelectMovieAsync(userId, sessionId, request, cancellationToken);
            return Ok(session);
        }
        catch (SessionNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotSessionMemberException)
        {
            return Forbid();
        }
        catch (InvalidSessionModeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (SessionNotReadyException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (NotYourTurnException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (PreviousMovieNotWatchedException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{sessionId:guid}/leave")]
    public async Task<IActionResult> LeaveSession(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await sessionService.LeaveSessionAsync(userId, sessionId, cancellationToken);
            return NoContent();
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
}
