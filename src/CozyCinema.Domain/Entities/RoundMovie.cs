namespace CozyCinema.Domain.Entities;

public class RoundMovie
{
    public Guid Id { get; set; }
    public Guid VotingRoundId { get; set; }
    public VotingRound VotingRound { get; set; } = null!;

    public Guid SuggestedByUserId { get; set; }
    public User SuggestedByUser { get; set; } = null!;

    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
