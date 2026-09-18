using CozyCinema.Domain.Enums;

namespace CozyCinema.Application.Sessions.Dtos;

public class SessionMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MemberStatus Status { get; set; }
}
