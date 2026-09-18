using CozyCinema.Api.Extensions;
using CozyCinema.Application.Sessions.Exceptions;
using CozyCinema.Application.Voting;
using CozyCinema.Application.Voting.Dtos;
using CozyCinema.Application.Voting.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyCinema.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/sessions/{sessionId:guid}/grupo")]
public class GroupVotingController(IVotingService votingService) : ControllerBase
{
    [HttpPost("movies")]
    public async Task<ActionResult<VotingRoundDto>> SuggestMovie(Guid sessionId, SuggestMovieRequestDto request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var round = await votingService.SuggestMovieAsync(userId, sessionId, request, cancellationToken);
            return Ok(round);
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
        catch (RoundNotInSelectionPhaseException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (AlreadySuggestedInRoundException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("ready")]
    public async Task<ActionResult<VotingRoundDto>> MarkReady(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var round = await votingService.MarkReadyAsync(userId, sessionId, cancellationToken);
            return Ok(round);
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
        catch (RoundNotInSelectionPhaseException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("votes")]
    public async Task<ActionResult<VotingRoundDto>> CastVote(Guid sessionId, CastVoteRequestDto request, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var round = await votingService.CastVoteAsync(userId, sessionId, request, cancellationToken);
            return Ok(round);
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
        catch (RoundNotInVotingPhaseException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (RoundMovieNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (SelfVoteNotAllowedException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (DuplicateVoteException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("round")]
    public async Task<ActionResult<VotingRoundDto?>> GetCurrentRound(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var round = await votingService.GetCurrentRoundAsync(userId, sessionId, cancellationToken);
            return Ok(round);
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
    }
}
