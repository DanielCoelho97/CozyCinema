namespace CozyCinema.Domain.Entities;

public class Vote
{
    public Guid Id { get; set; }

    public Guid RoundMovieId { get; set; }
    public RoundMovie RoundMovie { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
