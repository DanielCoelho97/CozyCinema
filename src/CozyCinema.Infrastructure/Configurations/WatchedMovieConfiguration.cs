using CozyCinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CozyCinema.Infrastructure.Configurations;

public class WatchedMovieConfiguration : IEntityTypeConfiguration<WatchedMovie>
{
    public void Configure(EntityTypeBuilder<WatchedMovie> builder)
    {
        builder.HasKey(wm => wm.Id);

        builder.Property(wm => wm.Title).IsRequired();
        builder.Property(wm => wm.Rating).HasColumnType("decimal(3,1)");

        builder.HasOne(wm => wm.Session)
            .WithMany(s => s.WatchedMovies)
            .HasForeignKey(wm => wm.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wm => wm.User)
            .WithMany(u => u.WatchedMovies)
            .HasForeignKey(wm => wm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
