namespace CozyCinema.Domain.Entities;

public class WatchedMovie
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Synopsis { get; set; }
    public decimal Rating { get; set; }
    public string Genres { get; set; } = string.Empty;
    public string? Director { get; set; }
    public int? ReleaseYear { get; set; }
    public DateTime WatchedAt { get; set; }
}
