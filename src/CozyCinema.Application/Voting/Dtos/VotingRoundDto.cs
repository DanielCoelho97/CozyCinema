using CozyCinema.Domain.Enums;

namespace CozyCinema.Application.Voting.Dtos;

public class VotingRoundDto
{
    public Guid Id { get; set; }
    public RoundStatus Status { get; set; }
    public List<RoundMovieDto> Movies { get; set; } = [];

    public int ReadyCount { get; set; }
    public int TotalActiveMembers { get; set; }
    public bool IsCurrentUserReady { get; set; }
    public bool HasCurrentUserVoted { get; set; }

    public Guid? WinnerMovieId { get; set; }
}
