using MediatR;
using ParkingService.Application.Features.Access.DTOs;

namespace ParkingService.Application.Features.Access.Queries.TestPlateComparison;

/// <summary>
/// API tiện ích để kiểm thử thuật toán đối chiếu 2 biển số xe với ngưỡng % tùy chọn.
/// </summary>
public record TestPlateComparisonQuery(
    string PlateA,
    string PlateB,
    double ThresholdPercent = 80.0
) : IRequest<PlateComparisonDto>;
