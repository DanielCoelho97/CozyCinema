using CozyCinema.Domain.Enums;

namespace CozyCinema.Domain.Entities;

public class SessionMember
{
    public Guid SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public MemberStatus Status { get; set; } = MemberStatus.Active;
}
