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

        // Chuyá»ƒn Ä‘á»•i GUID sang chuá»—i in hoa (UPPERCASE) vÃ  thiáº¿t láº­p collation NOCASE
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

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Chá»‰ má»¥c duy nháº¥t cho mÃ£ tháº»
        builder.HasIndex(c => c.CardNumber)
            .IsUnique();

        // Báº¥t biáº¿n váº­t lÃ½: Filtered Unique Index trÃªn UserId (chá»‰ Ã¡p dá»¥ng khi UserId IS NOT NULL)
        // 1 CÆ° dÃ¢n chá»‰ cÃ³ tá»‘i Ä‘a 1 tháº» xe trong toÃ n bá»™ há»‡ thá»‘ng
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        // KhÃ³a ngoáº¡i liÃªn káº¿t tá»›i báº£ng Users
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}