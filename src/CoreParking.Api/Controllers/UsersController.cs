using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreParking.Application.Common.Models;
using CoreParking.Application.Features.Users.DTOs;
using CoreParking.Domain.Enums;
using CoreParking.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ParkingDbContext _dbContext;

    public UsersController(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Láº¥y danh sÃ¡ch cÆ° dÃ¢n há»— trá»£ phÃ¢n trang (10/20 items), tÃ¬m kiáº¿m Ä‘a trÆ°á»ng vÃ  lá»c tráº¡ng thÃ¡i tháº».
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = "all",
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 1000);

        var baseUsersQuery = _dbContext.Users.AsNoTracking();

        // 1. TÃ¬m kiáº¿m Ä‘a trÆ°á»ng (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            baseUsersQuery = baseUsersQuery.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                (u.Phone != null && u.Phone.ToLower().Contains(term)) ||
                (u.PlateNumber != null && u.PlateNumber.ToLower().Contains(term)) ||
                (u.Room != null && u.Room.ToLower().Contains(term)));
        }

        // 2. Left Join vá»›i báº£ng Cards Ä‘á»ƒ láº¥y thÃ´ng tin tháº» liÃªn káº¿t (Active)
        var joinedQuery = baseUsersQuery.GroupJoin(
            _dbContext.Cards.AsNoTracking().Where(c => c.Status == CardStatus.Active),
            u => u.Id,
            c => c.UserId,
            (user, cards) => new { User = user, Card = cards.FirstOrDefault() }
        );

        // 3. Lá»c theo tráº¡ng thÃ¡i tháº»
        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (status.Equals("unassigned", StringComparison.OrdinalIgnoreCase) || 
                status.Equals("no-card", StringComparison.OrdinalIgnoreCase))
            {
                joinedQuery = joinedQuery.Where(x => x.Card == null);
            }
            else if (status.Equals("assigned", StringComparison.OrdinalIgnoreCase) || 
                     status.Equals("has-card", StringComparison.OrdinalIgnoreCase))
            {
                joinedQuery = joinedQuery.Where(x => x.Card != null);
            }
        }

        var totalCount = await joinedQuery.CountAsync(cancellationToken);

        var items = await joinedQuery
            .OrderBy(x => x.User.FullName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserDto(
                x.User.Id,
                x.User.FullName,
                x.User.Phone,
                x.User.PlateNumber,
                x.User.Room,
                x.Card != null ? x.Card.Id : null,
                x.Card != null ? x.Card.CardNumber : null
            ))
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<UserDto>(items, totalCount, pageNumber, pageSize);
        return Ok(ApiResponse<PaginatedResult<UserDto>>.Ok(result));
    }

    /// <summary>
    /// Láº¥y chi tiáº¿t má»™t cÆ° dÃ¢n theo Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user == null)
            return NotFound(ApiResponse<UserDto?>.Fail("KhÃ´ng tÃ¬m tháº¥y thÃ´ng tin cÆ° dÃ¢n.", "USER_NOT_FOUND"));

        var card = await _dbContext.Cards.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == id && c.Status == CardStatus.Active, cancellationToken);

        var dto = new UserDto(
            user.Id,
            user.FullName,
            user.Phone,
            user.PlateNumber,
            user.Room,
            card?.Id,
            card?.CardNumber
        );

        return Ok(ApiResponse<UserDto>.Ok(dto));
    }
}