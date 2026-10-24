using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingService.Domain.Entities;

namespace ParkingService.Infrastructure.Persistence.Configurations;

public class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("ParkingSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CardNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.CardNumber);

        builder.Property(s => s.CardId)
            .IsRequired(false);

        builder.Property(s => s.UserId)
            .IsRequired(false);

        builder.Property(s => s.PlateNumber)
            .HasMaxLength(30);

        builder.HasIndex(s => s.PlateNumber);

        builder.Property(s => s.EntryTime)
            .IsRequired();

        builder.Property(s => s.ExitTime);

        builder.Property(s => s.DurationMinutes);

        builder.Property(s => s.EntryEventId)
            .IsRequired(false);

        builder.Property(s => s.ExitEventId)
            .IsRequired(false);

        builder.Property(s => s.EntrySnapshotPath)
            .HasMaxLength(255);

        builder.Property(s => s.ExitSnapshotPath)
            .HasMaxLength(255);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(s => s.Status);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.Property(s => s.CreatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(s => s.IsDeleted)
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(s => s.Card)
            .WithMany()
            .HasForeignKey(s => s.CardId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(s => s.EntryEvent)
            .WithMany()
            .HasForeignKey(s => s.EntryEventId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(s => s.ExitEvent)
            .WithMany()
            .HasForeignKey(s => s.ExitEventId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Soft delete query filter
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
