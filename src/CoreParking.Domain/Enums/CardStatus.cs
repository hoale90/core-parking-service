namespace CoreParking.Domain.Enums;

/// <summary>
/// Tráº¡ng thÃ¡i hoáº¡t Ä‘á»™ng cá»§a tháº» RFID/IC.
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// Tháº» Ä‘ang hoáº¡t Ä‘á»™ng bÃ¬nh thÆ°á»ng, sáºµn sÃ ng cáº¥p phÃ¡t hoáº·c sá»­ dá»¥ng.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Tháº» bá»‹ táº¡m khÃ³a do vi pháº¡m, tranh cháº¥p hoáº·c yÃªu cáº§u báº£o máº­t.
    /// </summary>
    Locked = 2,

    /// <summary>
    /// Tháº» bá»‹ bÃ¡o máº¥t tá»« cÆ° dÃ¢n hoáº·c ban quáº£n lÃ½.
    /// </summary>
    Lost = 3
}