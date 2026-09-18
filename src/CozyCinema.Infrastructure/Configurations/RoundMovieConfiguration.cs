using CozyCinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CozyCinema.Infrastructure.Configurations;

public class RoundMovieConfiguration : IEntityTypeConfiguration<RoundMovie>
{
    public void Configure(EntityTypeBuilder<RoundMovie> builder)
    {
        builder.HasKey(rm => rm.Id);

        builder.Property(rm => rm.Title).IsRequired();

        builder.HasOne(rm => rm.VotingRound)
            .WithMany(vr => vr.Movies)
            .HasForeignKey(rm => rm.VotingRoundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rm => rm.SuggestedByUser)
            .WithMany()
            .HasForeignKey(rm => rm.SuggestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
