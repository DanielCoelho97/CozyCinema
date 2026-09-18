namespace CozyCinema.Application.Sessions.Dtos;

public class SessionCurrentMovieDto
{
    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public Guid PickedByUserId { get; set; }
}
