using CozyCinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CozyCinema.Infrastructure.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired();

        builder.Property(s => s.InviteCode).IsRequired().HasMaxLength(8);
        builder.HasIndex(s => s.InviteCode).IsUnique();

        builder.HasOne(s => s.LastPicker)
            .WithMany()
            .HasForeignKey(s => s.LastPickerUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
