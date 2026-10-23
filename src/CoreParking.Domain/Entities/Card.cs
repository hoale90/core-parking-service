using System;
using CoreParking.Domain.Enums;

namespace CoreParking.Domain.Entities;

/// <summary>
/// Thá»±c thá»ƒ Tháº» xe (Card).
/// Báº¥t biáº¿n quan trá»ng: 1 User chá»‰ Ä‘Æ°á»£c liÃªn káº¿t tá»‘i Ä‘a 1 tháº» xe táº¡i má»™t thá»i Ä‘iá»ƒm.
/// </summary>
public class Card
{
    public Guid Id { get; private set; }
    public string CardNumber { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public CardType Type { get; private set; } = CardType.Resident;
    public CardStatus Status { get; private set; } = CardStatus.Active;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // EF Core Constructor
    private Card() { }

    public Card(Guid id, string cardNumber, CardType type = CardType.Resident, CardStatus status = CardStatus.Active)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Card Id cannot be empty.", nameof(id));

        Id = id;
        CardNumber = !string.IsNullOrWhiteSpace(cardNumber)
            ? cardNumber.Trim()
            : throw new ArgumentException("Card number is required.", nameof(cardNumber));

        Type = type;
        Status = status;
        UserId = null;
        CreatedAt = DateTime.UtcNow;
    }

    public void AssignToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Target User Id cannot be empty.", nameof(userId));

        if (Status != CardStatus.Active)
            throw new InvalidOperationException($"KhÃ´ng thá»ƒ gÃ¡n tháº» {CardNumber} vÃ¬ tháº» Ä‘ang á»Ÿ tráº¡ng thÃ¡i {Status}.");

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
            // Tá»± Ä‘á»™ng giáº£i phÃ³ng liÃªn káº¿t náº¿u tháº» bá»‹ khÃ³a hoáº·c bÃ¡o máº¥t
            UserId = null;
        }
    }
}