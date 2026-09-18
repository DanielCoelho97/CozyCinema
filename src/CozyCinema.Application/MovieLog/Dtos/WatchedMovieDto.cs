namespace CozyCinema.Application.MovieLog.Dtos;

public class WatchedMovieDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Synopsis { get; set; }
    public decimal Rating { get; set; }
    public List<string> Genres { get; set; } = [];
    public string? Director { get; set; }
    public int? ReleaseYear { get; set; }
    public DateTime WatchedAt { get; set; }
}
