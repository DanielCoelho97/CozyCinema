using CozyCinema.Domain.Enums;

namespace CozyCinema.Domain.Entities;

public class Session
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public SessionMode Mode { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    public Guid? LastPickerUserId { get; set; }
    public User? LastPicker { get; set; }

    public int? CurrentMovieTmdbId { get; set; }
    public string? CurrentMovieTitle { get; set; }
    public string? CurrentMovieCoverUrl { get; set; }

    public ICollection<SessionMember> Members { get; set; } = new List<SessionMember>();
    public ICollection<VotingRound> VotingRounds { get; set; } = new List<VotingRound>();
    public ICollection<WatchedMovie> WatchedMovies { get; set; } = new List<WatchedMovie>();
}
