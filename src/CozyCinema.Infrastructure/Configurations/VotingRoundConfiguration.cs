using CozyCinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CozyCinema.Infrastructure.Configurations;

public class VotingRoundConfiguration : IEntityTypeConfiguration<VotingRound>
{
    public void Configure(EntityTypeBuilder<VotingRound> builder)
    {
        builder.HasKey(vr => vr.Id);

        builder.HasOne(vr => vr.Session)
            .WithMany(s => s.VotingRounds)
            .HasForeignKey(vr => vr.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(vr => vr.WinnerMovie)
            .WithMany()
            .HasForeignKey(vr => vr.WinnerMovieId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
