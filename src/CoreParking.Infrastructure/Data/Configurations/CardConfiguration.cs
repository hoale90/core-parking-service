using System;
using CoreParking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreParking.Infrastructure.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards");

        builder.HasKey(c => c.Id);

        // Chuyển đổi GUID sang chuỗi in hoa (UPPERCASE) và thiết lập collation NOCASE
        builder.Property(c => c.Id)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Guid.Parse(v))
            .UseCollation("NOCASE")
            .IsRequired();

        builder.Property(c => c.UserId)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString().ToUpperInvariant() : null,
                v => !string.IsNullOrEmpty(v) ? Guid.Parse(v) : (Guid?)null)
            .UseCollation("NOCASE");

        builder.Property(c => c.CardNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.Status)
            .IsRequired();

        builder.Property(c => c.SkipValidatePlate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Chỉ mục duy nhất cho mã thẻ
        builder.HasIndex(c => c.CardNumber)
            .IsUnique();

        // Bất biến vật lý: Filtered Unique Index trên UserId (chỉ áp dụng khi UserId IS NOT NULL)
        // 1 Cư dân chỉ có tối đa 1 thẻ xe trong toàn bộ hệ thống
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        // Khóa ngoại liên kết tới bảng Users
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
