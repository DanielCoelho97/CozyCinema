using CozyCinema.Application.Sessions.Dtos;

namespace CozyCinema.Application.Sessions;

public interface ISessionService
{
    Task<SessionDto> CreateSessionAsync(Guid userId, CreateSessionRequestDto request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SessionSummaryDto>> GetMySessionsAsync(Guid userId, CancellationToken cancellationToken);

    Task<SessionDto> JoinSessionAsync(Guid userId, JoinSessionRequestDto request, CancellationToken cancellationToken);

    Task<SessionDto> GetSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    Task<SessionDto> SelectMovieAsync(Guid userId, Guid sessionId, SelectMovieRequestDto request, CancellationToken cancellationToken);

    Task LeaveSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);
}
