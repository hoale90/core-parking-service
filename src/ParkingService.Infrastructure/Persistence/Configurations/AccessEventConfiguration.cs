using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingService.Domain.Entities;

namespace ParkingService.Infrastructure.Persistence.Configurations;

public class AccessEventConfiguration : IEntityTypeConfiguration<AccessEvent>
{
    public void Configure(EntityTypeBuilder<AccessEvent> builder)
    {
        builder.ToTable("AccessEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CardNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.CardNumber);

        builder.Property(e => e.CardId)
            .IsRequired(false);

        builder.Property(e => e.UserId)
            .IsRequired(false);

        builder.Property(e => e.Direction)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.LaneCode)
            .HasMaxLength(50);

        builder.Property(e => e.OcrPlateNumber)
            .HasMaxLength(30);

        builder.Property(e => e.PlateNormalized)
            .HasMaxLength(30);

        builder.Property(e => e.ExpectedPlateNumber)
            .HasMaxLength(30);

        builder.Property(e => e.PlateSimilarityPercent);

        builder.Property(e => e.SkipValidatePlate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.Decision)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ReasonCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.SnapshotPath)
            .HasMaxLength(255);

        builder.Property(e => e.EventTime)
            .IsRequired();

        builder.HasIndex(e => e.EventTime);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(e => e.Card)
            .WithMany()
            .HasForeignKey(e => e.CardId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Soft delete query filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
