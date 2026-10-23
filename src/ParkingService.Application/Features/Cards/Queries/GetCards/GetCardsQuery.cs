using MediatR;
using ParkingService.Application.Common.Models;
using ParkingService.Application.Features.Cards.DTOs;
using ParkingService.Domain.Enums;

namespace ParkingService.Application.Features.Cards.Queries.GetCards;

public record GetCardsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? UserId = null,
    string? Status = null,
    CardType? Type = null,
    string? SearchTerm = null
) : IRequest<PaginatedResult<CardDto>>;
