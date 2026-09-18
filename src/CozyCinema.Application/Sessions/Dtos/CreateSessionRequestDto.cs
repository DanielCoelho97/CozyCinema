using System.ComponentModel.DataAnnotations;
using CozyCinema.Domain.Enums;

namespace CozyCinema.Application.Sessions.Dtos;

public class CreateSessionRequestDto
{
    [Required]
    [MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public SessionMode Mode { get; set; }

    [MaxLength(2048)]
    public string? IconUrl { get; set; }
}
