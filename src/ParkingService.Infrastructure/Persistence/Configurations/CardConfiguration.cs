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

        builder.Property(c => c.VehiclePlate)
            .HasMaxLength(20);

        builder.Property(c => c.SkipValidateVehiclePlate)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(c => c.Info)
            .HasMaxLength(250);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Room)
            .HasMaxLength(50);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
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

        // Relationship: One User has many Cards, Card optionally belongs to one User
        builder.HasOne(c => c.User)
            .WithMany(u => u.Cards)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Soft delete query filter
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
