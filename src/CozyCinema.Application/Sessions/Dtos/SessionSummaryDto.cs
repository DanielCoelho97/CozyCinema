using CozyCinema.Domain.Enums;

namespace CozyCinema.Application.Sessions.Dtos;

public class SessionSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public SessionMode Mode { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public int ActiveMemberCount { get; set; }
}
