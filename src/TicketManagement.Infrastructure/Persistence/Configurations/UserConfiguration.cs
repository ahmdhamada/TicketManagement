using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.CreatedTickets)
            .WithOne(t => t.CreatedByUser)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedTickets)
            .WithOne(t => t.AssignedToUser)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
