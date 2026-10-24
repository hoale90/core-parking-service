using MediatR;
using ParkingService.Application.Common.Interfaces;
using ParkingService.Application.Features.Access.DTOs;

namespace ParkingService.Application.Features.Access.Queries.TestPlateComparison;

public class TestPlateComparisonQueryHandler : IRequestHandler<TestPlateComparisonQuery, PlateComparisonDto>
{
    private readonly IPlateComparisonService _plateComparisonService;

    public TestPlateComparisonQueryHandler(IPlateComparisonService plateComparisonService)
    {
        _plateComparisonService = plateComparisonService;
    }

    public Task<PlateComparisonDto> Handle(TestPlateComparisonQuery request, CancellationToken cancellationToken)
    {
        var result = _plateComparisonService.Compare(request.PlateA, request.PlateB, request.ThresholdPercent);

        var evaluation = result.IsMatch
            ? $"ĐẠT (HỢP LỆ): Độ tương đồng {result.SimilarityPercent}% >= ngưỡng {request.ThresholdPercent}%. Cho phép mở barrier."
            : $"TỪ CHỐI: Độ tương đồng {result.SimilarityPercent}% < ngưỡng {request.ThresholdPercent}%. Biển số không đủ điều kiện trùng khớp.";

        var dto = new PlateComparisonDto
        {
            DetectedPlate = request.PlateA,
            ExpectedPlate = request.PlateB,
            NormalizedDetected = result.NormalizedDetected,
            NormalizedExpected = result.NormalizedExpected,
            SimilarityPercent = result.SimilarityPercent,
            Distance = result.Distance,
            ThresholdPercent = request.ThresholdPercent,
            IsMatch = result.IsMatch,
            Evaluation = evaluation
        };

        return Task.FromResult(dto);
    }
}
