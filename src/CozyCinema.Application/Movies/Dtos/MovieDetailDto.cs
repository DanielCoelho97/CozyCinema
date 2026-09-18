namespace CozyCinema.Application.Movies.Dtos;

public class MovieDetailDto
{
    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public string? Synopsis { get; set; }
    public decimal? Rating { get; set; }
    public List<string> Genres { get; set; } = [];
    public string? Director { get; set; }
    public DateOnly? ReleaseDate { get; set; }
}
