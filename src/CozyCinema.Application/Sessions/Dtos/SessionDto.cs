using CozyCinema.Domain.Enums;

namespace CozyCinema.Application.Sessions.Dtos;

public class SessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public SessionMode Mode { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    public Guid? LastPickerUserId { get; set; }
    public Guid? NextPickerUserId { get; set; }

    public SessionCurrentMovieDto? CurrentMovie { get; set; }

    public List<SessionMemberDto> Members { get; set; } = [];
}
