using CozyCinema.Application.Voting.Dtos;

namespace CozyCinema.Application.Voting;

public interface IVotingService
{
    Task<VotingRoundDto> SuggestMovieAsync(Guid userId, Guid sessionId, SuggestMovieRequestDto request, CancellationToken cancellationToken);

    Task<VotingRoundDto> MarkReadyAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    Task<VotingRoundDto> CastVoteAsync(Guid userId, Guid sessionId, CastVoteRequestDto request, CancellationToken cancellationToken);

    Task<VotingRoundDto?> GetCurrentRoundAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
}
