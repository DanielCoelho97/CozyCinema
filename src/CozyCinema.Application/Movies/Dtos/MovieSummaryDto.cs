namespace CozyCinema.Application.Movies.Dtos;

public class MovieSummaryDto
{
    public int TmdbMovieId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }
    public int? ReleaseYear { get; set; }
    public decimal? Rating { get; set; }
}
