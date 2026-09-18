using System.ComponentModel.DataAnnotations;

namespace CozyCinema.Application.Sessions.Dtos;

public class SelectMovieRequestDto
{
    [Required]
    public int TmdbMovieId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string? CoverUrl { get; set; }
}
