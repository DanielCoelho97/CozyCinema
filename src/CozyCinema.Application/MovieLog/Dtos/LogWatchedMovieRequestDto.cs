using System.ComponentModel.DataAnnotations;

namespace CozyCinema.Application.MovieLog.Dtos;

public class LogWatchedMovieRequestDto
{
    [Required]
    public int TmdbMovieId { get; set; }

    public DateTime? WatchedAt { get; set; }
}
