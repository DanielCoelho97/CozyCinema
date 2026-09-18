namespace CozyCinema.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<SessionMember> SessionMemberships { get; set; } = new List<SessionMember>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<WatchedMovie> WatchedMovies { get; set; } = new List<WatchedMovie>();
}
