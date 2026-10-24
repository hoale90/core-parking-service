using System;
using CoreParking.Domain.Enums;

namespace CoreParking.Domain.Entities;

/// <summary>
/// Thực thể Thẻ xe (Card).
/// Bất biến quan trọng: 1 User chỉ được liên kết tối đa 1 thẻ xe tại một thời điểm.
/// </summary>
public class Card
{
    public Guid Id { get; private set; }
    public string CardNumber { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public CardType Type { get; private set; } = CardType.Resident;
    public CardStatus Status { get; private set; } = CardStatus.Active;
    public bool SkipValidatePlate { get; private set; } = false;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core Constructor
    private Card() { }

    public Card(Guid id, string cardNumber, CardType type = CardType.Resident, CardStatus status = CardStatus.Active, bool skipValidatePlate = false)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Card Id cannot be empty.", nameof(id));

        Id = id;
        CardNumber = !string.IsNullOrWhiteSpace(cardNumber)
            ? cardNumber.Trim()
            : throw new ArgumentException("Card number is required.", nameof(cardNumber));

        Type = type;
        Status = status;
        SkipValidatePlate = skipValidatePlate;
        UserId = null;
        CreatedAt = DateTime.UtcNow;
    }

    public void AssignToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Target User Id cannot be empty.", nameof(userId));

        if (Status != CardStatus.Active)
            throw new InvalidOperationException($"Không thể gán thẻ {CardNumber} vì thẻ đang ở trạng thái {Status}.");

        UserId = userId;
    }

    public void Unassign()
    {
        UserId = null;
    }

    public void ChangeStatus(CardStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == CardStatus.Locked || newStatus == CardStatus.Lost)
        {
            // Tự động giải phóng liên kết nếu thẻ bị khóa hoặc báo mất
            UserId = null;
        }
    }

    public void SetSkipValidatePlate(bool skip)
    {
        SkipValidatePlate = skip;
    }
}
