using CozyCinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CozyCinema.Infrastructure.Configurations;

public class SessionMemberConfiguration : IEntityTypeConfiguration<SessionMember>
{
    public void Configure(EntityTypeBuilder<SessionMember> builder)
    {
        builder.HasKey(sm => new { sm.SessionId, sm.UserId });

        builder.HasOne(sm => sm.Session)
            .WithMany(s => s.Members)
            .HasForeignKey(sm => sm.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.User)
            .WithMany(u => u.SessionMemberships)
            .HasForeignKey(sm => sm.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
