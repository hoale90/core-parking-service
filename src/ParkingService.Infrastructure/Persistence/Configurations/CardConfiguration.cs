using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingService.Domain.Entities;
using ParkingService.Domain.Enums;

namespace ParkingService.Infrastructure.Persistence.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Unique index for CardNumber
        builder.HasIndex(c => c.CardNumber)
            .IsUnique();

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CardType.Registered)
            .HasSentinel((CardType)0)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion(
                v => v == CardStatus.Active ? "active" : "inactive",
                v => v.ToLower() == "active" || v == "1" ? CardStatus.Active : CardStatus.Inactive)
            .HasMaxLength(20)
            .HasDefaultValue(CardStatus.Active)
            .HasSentinel((CardStatus)0)
            .IsRequired();

        builder.Property(c => c.UserId)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        // Relationship: One User can have Cards, Card optionally belongs to one User
        builder.HasOne(c => c.User)
            .WithMany(u => u.Cards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Soft delete query filter
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
