using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Body).HasMaxLength(2000).IsRequired();

        builder.HasOne(c => c.AuthorUser)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TicketId);
    }
}
