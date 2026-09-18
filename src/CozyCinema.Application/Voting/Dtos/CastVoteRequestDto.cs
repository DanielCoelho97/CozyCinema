using System.ComponentModel.DataAnnotations;

namespace CozyCinema.Application.Voting.Dtos;

public class CastVoteRequestDto
{
    [Required]
    public Guid RoundMovieId { get; set; }
}
