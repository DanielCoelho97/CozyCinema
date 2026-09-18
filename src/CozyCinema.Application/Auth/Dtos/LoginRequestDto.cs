using System.ComponentModel.DataAnnotations;

namespace CozyCinema.Application.Auth.Dtos;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
