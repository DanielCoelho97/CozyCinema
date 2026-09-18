using System.ComponentModel.DataAnnotations;

namespace CozyCinema.Application.Sessions.Dtos;

public class JoinSessionRequestDto
{
    [Required]
    [MaxLength(8)]
    public string InviteCode { get; set; } = string.Empty;
}
