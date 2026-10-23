using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreParking.Application.Common.Models;
using CoreParking.Application.Features.Cards.Commands;
using CoreParking.Application.Features.Cards.DTOs;
using CoreParking.Domain.Enums;
using CoreParking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly ParkingDbContext _dbContext;

    public CardsController(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Láº¥y danh má»¥c tháº» xe há»— trá»£ phÃ¢n trang (10/20 items), tÃ¬m kiáº¿m mÃ£ tháº» vÃ  lá»c tráº¡ng thÃ¡i.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = "all",
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 1000);

        var baseCardsQuery = _dbContext.Cards.AsNoTracking();

        // 1. TÃ¬m kiáº¿m theo mÃ£ tháº»
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            baseCardsQuery = baseCardsQuery.Where(c => c.CardNumber.ToLower().Contains(term));
        }

        // 2. Lá»c theo tráº¡ng thÃ¡i
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (status.Equals("available", StringComparison.OrdinalIgnoreCase) || 
                status.Equals("unassigned", StringComparison.OrdinalIgnoreCase))
            {
                baseCardsQuery = baseCardsQuery.Where(c => c.UserId == null && c.Status == CardStatus.Active);
            }
            else if (status.Equals("assigned", StringComparison.OrdinalIgnoreCase))
            {
                baseCardsQuery = baseCardsQuery.Where(c => c.UserId != null);
            }
            else if (status.Equals("locked", StringComparison.OrdinalIgnoreCase))
            {
                baseCardsQuery = baseCardsQuery.Where(c => c.Status == CardStatus.Locked);
            }
            else if (status.Equals("lost", StringComparison.OrdinalIgnoreCase))
            {
                baseCardsQuery = baseCardsQuery.Where(c => c.Status == CardStatus.Lost);
            }
        }

        // 3. Left Join vá»›i báº£ng Users Ä‘á»ƒ hiá»ƒn thá»‹ há» tÃªn cÆ° dÃ¢n Ä‘ang gÃ¡n
        var joinedQuery = baseCardsQuery.GroupJoin(
            _dbContext.Users.AsNoTracking(),
            c => c.UserId,
            u => u.Id,
            (card, users) => new { Card = card, User = users.FirstOrDefault() }
        );

        var totalCount = await joinedQuery.CountAsync(cancellationToken);

        var items = await joinedQuery
            .OrderBy(x => x.Card.CardNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CardDto(
                x.Card.Id,
                x.Card.CardNumber,
                x.Card.UserId,
                (int)x.Card.Type,
                (int)x.Card.Status,
                x.User != null ? x.User.FullName : null
            ))
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<CardDto>(items, totalCount, pageNumber, pageSize);
        return Ok(ApiResponse<PaginatedResult<CardDto>>.Ok(result));
    }

    /// <summary>
    /// GÃ¡n hoáº·c há»§y gÃ¡n tháº» xe cho cÆ° dÃ¢n: PUT /api/cards/{cardId}/assign-user
    /// Quy táº¯c:
    /// - Náº¿u request.UserId == null: Há»§y gÃ¡n tháº» (thu há»“i vá» kho).
    /// - Náº¿u request.UserId != null:
    ///     + Kiá»ƒm tra tháº» cÃ³ Active khÃ´ng. Náº¿u khÃ´ng -> 400 Bad Request.
    ///     + Kiá»ƒm tra User cÃ³ tá»“n táº¡i khÃ´ng. Náº¿u khÃ´ng -> 404 Not Found.
    ///     + Chá»‘ng gÃ¡n trÃ¹ng: Náº¿u tháº» Ä‘Ã£ gÃ¡n cho ngÆ°á»i khÃ¡c -> 409 Conflict.
    ///     + Báº¥t biáº¿n 1 cÆ° dÃ¢n - 1 tháº»: Náº¿u cÆ° dÃ¢n Ä‘ang sá»Ÿ há»¯u tháº» cÅ© khÃ¡c, tá»± Ä‘á»™ng há»§y gÃ¡n tháº» cÅ© qua Two-Phase Save.
    ///     + Báº¯t ngoáº¡i lá»‡ DbUpdateException khi tranh cháº¥p Ä‘á»“ng thá»i -> 409 Conflict CONCURRENT_CONFLICT.
    /// </summary>
    [HttpPut("{cardId:guid}/assign-user")]
    public async Task<IActionResult> AssignUser(
        Guid cardId,
        [FromBody] AssignUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var card = await _dbContext.Cards.FirstOrDefaultAsync(c => c.Id == cardId, cancellationToken);
        if (card == null)
        {
            return NotFound(ApiResponse<object?>.Fail("Tháº» khÃ´ng tá»“n táº¡i trong há»‡ thá»‘ng.", "CARD_NOT_FOUND"));
        }

        // TRÆ¯á»œNG Há»¢P 1: Há»¦Y GÃN THáºº (UserId == null)
        if (!request.UserId.HasValue)
        {
            card.Unassign();
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse<object?>.Ok(null));
        }

        var targetUserId = request.UserId.Value;

        // TRÆ¯á»œNG Há»¢P 2: GÃN THáºº CHO CÆ¯ DÃ‚N
        if (card.Status != CardStatus.Active)
        {
            return BadRequest(ApiResponse<object?>.Fail(
                $"Tháº» {card.CardNumber} Ä‘ang á»Ÿ tráº¡ng thÃ¡i {card.Status}, khÃ´ng thá»ƒ cáº¥p phÃ¡t.", 
                "CARD_NOT_ACTIVE"));
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);
        if (user == null)
        {
            return NotFound(ApiResponse<object?>.Fail("KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin cÆ° dÃ¢n.", "USER_NOT_FOUND"));
        }

        // Cháº·n gÃ¡n trÃ¹ng: Náº¿u tháº» Ä‘Ã£ Ä‘Æ°á»£c gÃ¡n cho ngÆ°á»i khÃ¡c
        if (card.UserId.HasValue && card.UserId.Value != targetUserId)
        {
            var currentHolder = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == card.UserId.Value, cancellationToken);
            var holderName = currentHolder?.FullName ?? "cÆ° dÃ¢n khÃ¡c";

            return Conflict(ApiResponse<object?>.Fail(
                $"Tháº» {card.CardNumber} Ä‘Ã£ Ä‘Æ°á»£c gÃ¡n cho {holderName}.", 
                "CARD_ALREADY_ASSIGNED"));
        }

        // Äá»•i tháº» an toÃ n (Báº¥t biáº¿n 1 Resident - 1 Card):
        // Náº¿u cÆ° dÃ¢n Ä‘ang giá»¯ tháº» cÅ© khÃ¡c trong há»‡ thá»‘ng, thá»±c hiá»‡n Two-Phase Save:
        // Giai Ä‘oáº¡n 1: Giáº£i phÃ³ng tháº» cÅ© vÃ  lÆ°u thay Ä‘á»•i xuá»‘ng DB Ä‘á»ƒ nháº£ Unique Index IX_Cards_UserId
        var oldCards = await _dbContext.Cards
            .Where(c => c.UserId == targetUserId && c.Id != cardId)
            .ToListAsync(cancellationToken);

        if (oldCards.Count > 0)
        {
            foreach (var old in oldCards)
            {
                old.Unassign();
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Giai Ä‘oáº¡n 2: GÃ¡n tháº» má»›i vÃ  lÆ°u thay Ä‘á»•i vá»›i bá»c try ... catch xá»­ lÃ½ tranh cháº¥p Ä‘á»“ng thá»i
        try
        {
            card.AssignToUser(targetUserId);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Ok(ApiResponse<object?>.Ok(null));
        }
        catch (DbUpdateException)
        {
            return Conflict(ApiResponse<object?>.Fail("Tháº» hoáº·c cÆ° dÃ¢n Ä‘Ã£ bá»‹ gÃ¡n trong phiÃªn lÃ m viá»‡c khÃ¡c", "CONCURRENT_CONFLICT"));
        }
    }

    /// <summary>
    /// Cáº­p nháº­t tráº¡ng thÃ¡i tháº»: PUT /api/cards/{cardId}/status
    /// Khi tráº¡ng thÃ¡i chuyá»ƒn sang Locked (2) hoáº·c Lost (3), tá»± Ä‘á»™ng thu há»“i tháº» (UserId = null).
    /// </summary>
    [HttpPut("{cardId:guid}/status")]
    public async Task<IActionResult> UpdateCardStatus(
        Guid cardId,
        [FromBody] UpdateCardStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var card = await _dbContext.Cards.FirstOrDefaultAsync(c => c.Id == cardId, cancellationToken);
        if (card == null)
        {
            return NotFound(ApiResponse<object?>.Fail("Tháº» khÃ´ng tá»“n táº¡i trong há»‡ thá»‘ng.", "CARD_NOT_FOUND"));
        }

        if (!Enum.IsDefined(typeof(CardStatus), request.Status))
        {
            return BadRequest(ApiResponse<object?>.Fail("MÃ£ tráº¡ng thÃ¡i tháº» khÃ´ng há»£p lá»‡.", "INVALID_STATUS"));
        }

        var newStatus = (CardStatus)request.Status;
        card.ChangeStatus(newStatus);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null));
    }
}