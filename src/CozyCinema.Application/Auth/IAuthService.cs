using CozyCinema.Application.Auth.Dtos;

namespace CozyCinema.Application.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);

    Task<UserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
