using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

public class TicketActivityConfiguration : IEntityTypeConfiguration<TicketActivity>
{
    public void Configure(EntityTypeBuilder<TicketActivity> builder)
    {
        builder.ToTable("TicketActivities");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.OldValue).HasMaxLength(500);
        builder.Property(a => a.NewValue).HasMaxLength(500);
        builder.Property(a => a.Description).HasMaxLength(1000);

        builder.HasOne(a => a.ActorUser)
            .WithMany()
            .HasForeignKey(a => a.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.TicketId);
    }
}
