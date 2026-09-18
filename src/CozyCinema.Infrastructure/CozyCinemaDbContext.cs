using CozyCinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CozyCinema.Infrastructure;

public class CozyCinemaDbContext : DbContext
{
    public CozyCinemaDbContext(DbContextOptions<CozyCinemaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionMember> SessionMembers => Set<SessionMember>();
    public DbSet<VotingRound> VotingRounds => Set<VotingRound>();
    public DbSet<RoundMovie> RoundMovies => Set<RoundMovie>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<WatchedMovie> WatchedMovies => Set<WatchedMovie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CozyCinemaDbContext).Assembly);
    }
}
