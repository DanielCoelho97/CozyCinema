using CozyCinema.Domain.Enums;

namespace CozyCinema.Domain.Entities;

public class VotingRound
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public RoundStatus Status { get; set; } = RoundStatus.Selecting;

    public DateTime CreatedAt { get; set; }

    public Guid? WinnerMovieId { get; set; }
    public RoundMovie? WinnerMovie { get; set; }

    public ICollection<RoundMovie> Movies { get; set; } = new List<RoundMovie>();
}
