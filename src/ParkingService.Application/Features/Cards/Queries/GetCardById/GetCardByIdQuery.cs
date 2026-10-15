using MediatR;
using ParkingService.Application.Features.Cards.DTOs;

namespace ParkingService.Application.Features.Cards.Queries.GetCardById;

public record GetCardByIdQuery(Guid Id) : IRequest<CardDto>;
