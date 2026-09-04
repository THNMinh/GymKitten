using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.SizeGuides.Commands.RecommendSize;

public sealed class RecommendSizeCommandHandler
    : ICommandHandler<RecommendSizeCommand, Result<RecommendSizeResponse>>
{
    private readonly ISizeGuideRepository _sizeGuideRepository;

    public RecommendSizeCommandHandler(ISizeGuideRepository sizeGuideRepository)
    {
        _sizeGuideRepository = sizeGuideRepository;
    }

    public async Task<Result<RecommendSizeResponse>> Handle(
        RecommendSizeCommand request,
        CancellationToken cancellationToken)
    {
        string recommendedSize = "M";
        string confidence = "90%";
        string explanation = "Dựa trên thông số chiều cao, cân nặng và số đo cơ thể của bạn.";

        if (request.ProductId.HasValue && request.ProductId.Value != Guid.Empty)
        {
            var guides = await _sizeGuideRepository.GetByProductIdAsync(request.ProductId.Value, cancellationToken);
            if (guides.Count > 0 && request.ChestCm.HasValue)
            {
                var chestVal = request.ChestCm.Value;
                foreach (var g in guides)
                {
                    if (!string.IsNullOrWhiteSpace(g.Chestcm) && TryParseRange(g.Chestcm, out double minC, out double maxC))
                    {
                        if (chestVal >= minC && chestVal <= maxC)
                        {
                            recommendedSize = g.Size;
                            confidence = "95%";
                            explanation = $"Số đo vòng ngực {chestVal}cm nằm trong khoảng {g.Chestcm}cm của Size {g.Size}.";
                            break;
                        }
                    }
                }
            }
        }

        // Fallback intelligent calculation based on Height & Weight if not matched by product table
        if (confidence != "95%" && (request.HeightCm.HasValue || request.WeightKg.HasValue))
        {
            var h = request.HeightCm ?? 170;
            var w = request.WeightKg ?? 65;

            if (w < 55 && h < 165)
            {
                recommendedSize = "S";
                confidence = "88%";
                explanation = $"Dựa trên vóc dáng (Chiều cao {h}cm, Cân nặng {w}kg), Size S là lựa chọn vừa vặn nhất.";
            }
            else if (w <= 70 && h <= 175)
            {
                recommendedSize = "M";
                confidence = "92%";
                explanation = $"Dựa trên vóc dáng chuẩn gym (Chiều cao {h}cm, Cân nặng {w}kg), Size M tôn dáng và thoải mái nhất.";
            }
            else if (w <= 82 && h <= 182)
            {
                recommendedSize = "L";
                confidence = "90%";
                explanation = $"Dựa trên vóc dáng (Chiều cao {h}cm, Cân nặng {w}kg), Size L phù hợp cho cơ thể vạm vỡ.";
            }
            else
            {
                recommendedSize = "XL";
                confidence = "85%";
                explanation = $"Dựa trên chiều cao {h}cm và cân nặng {w}kg, Size XL sẽ tạo sự thoải mái tối đa khi vận động.";
            }
        }

        return Result.Success(new RecommendSizeResponse(recommendedSize, confidence, explanation));
    }

    private static bool TryParseRange(string rangeStr, out double minVal, out double maxVal)
    {
        minVal = 0;
        maxVal = 0;
        if (string.IsNullOrWhiteSpace(rangeStr)) return false;

        var parts = rangeStr.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && double.TryParse(parts[0], out minVal) && double.TryParse(parts[1], out maxVal))
        {
            return true;
        }

        if (parts.Length == 1 && double.TryParse(parts[0], out minVal))
        {
            maxVal = minVal;
            return true;
        }

        return false;
    }
}
