using System;
using CoreParking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreParking.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Chuyá»ƒn Ä‘á»•i GUID sang chuá»—i in hoa (UPPERCASE) vÃ  thiáº¿t láº­p collation NOCASE
        builder.Property(u => u.Id)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Guid.Parse(v))
            .UseCollation("NOCASE")
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.PlateNumber)
            .HasMaxLength(30);

        builder.Property(u => u.Room)
            .HasMaxLength(50);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}