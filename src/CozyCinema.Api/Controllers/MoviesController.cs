using CozyCinema.Application.Movies;
using CozyCinema.Application.Movies.Dtos;
using CozyCinema.Application.Movies.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyCinema.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/movies")]
public class MoviesController(ITmdbService tmdbService) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<MovieSummaryDto>>> Search(
        [FromQuery] string query,
        [FromQuery] int page,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(Array.Empty<MovieSummaryDto>());
        }

        try
        {
            var results = await tmdbService.SearchMoviesAsync(query.Trim(), page < 1 ? 1 : page, cancellationToken);
            return Ok(results);
        }
        catch (TmdbUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { message = ex.Message });
        }
    }

    [HttpGet("{tmdbId:int}")]
    public async Task<ActionResult<MovieDetailDto>> GetDetail(int tmdbId, CancellationToken cancellationToken)
    {
        try
        {
            var detail = await tmdbService.GetMovieDetailAsync(tmdbId, cancellationToken);
            return Ok(detail);
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
}
