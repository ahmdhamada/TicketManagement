using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(4000).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.CreatedByUserId);
        builder.HasIndex(t => t.AssignedToUserId);

        builder.HasMany(t => t.Comments).WithOne(c => c.Ticket).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.Activities).WithOne(a => a.Ticket).HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(t => t.TimeEntries).WithOne(te => te.Ticket).HasForeignKey(te => te.TicketId).OnDelete(DeleteBehavior.Cascade);
    }
}
