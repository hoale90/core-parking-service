using System;

namespace CoreParking.Domain.Entities;

/// <summary>
/// Thá»±c thá»ƒ CÆ° dÃ¢n (User) trong há»‡ thá»‘ng quáº£n lÃ½ bÃ£i xe.
/// TuÃ¢n thá»§ quy táº¯c DDD: Báº£o vá»‡ tÃ­nh toÃ n váº¹n nghiá»‡p vá»¥, khÃ´ng má»Ÿ public setters.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? PlateNumber { get; private set; }
    public string? Room { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core Constructor
    private User() { }

    public User(Guid id, string fullName, string? phone = null, string? plateNumber = null, string? room = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User Id cannot be empty.", nameof(id));

        Id = id;
        FullName = !string.IsNullOrWhiteSpace(fullName) 
            ? fullName.Trim() 
            : throw new ArgumentException("Full name is required.", nameof(fullName));

        Phone = phone?.Trim();
        PlateNumber = plateNumber?.Trim();
        Room = room?.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateInfo(string fullName, string? phone, string? plateNumber, string? room)
    {
        FullName = !string.IsNullOrWhiteSpace(fullName)
            ? fullName.Trim()
            : throw new ArgumentException("Full name is required.", nameof(fullName));

        Phone = phone?.Trim();
        PlateNumber = plateNumber?.Trim();
        Room = room?.Trim();
    }
}