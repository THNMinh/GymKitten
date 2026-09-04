using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.SizeGuides.Commands.UpdateSizeGuide;

public sealed class UpdateSizeGuideCommandHandler
    : ICommandHandler<UpdateSizeGuideCommand, Result<UpdateSizeGuideResponse>>
{
    private readonly IUserContext _userContext;
    private readonly ISizeGuideRepository _sizeGuideRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSizeGuideCommandHandler(
        IUserContext userContext,
        ISizeGuideRepository sizeGuideRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _sizeGuideRepository = sizeGuideRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateSizeGuideResponse>> Handle(
        UpdateSizeGuideCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UpdateSizeGuideResponse>(UserErrors.Forbidden);
        }

        var guide = await _sizeGuideRepository.GetByIdAsync(request.GuideId, cancellationToken);
        if (guide is null)
        {
            return Result.Failure<UpdateSizeGuideResponse>(SizeGuideErrors.NotFound);
        }

        guide.Size = request.Size.Trim().ToUpper();
        guide.Chestcm = request.ChestCm?.Trim();
        guide.Waistcm = request.WaistCm?.Trim();
        guide.Hipscm = request.HipsCm?.Trim();
        guide.Heightrangecm = request.HeightRangeCm?.Trim();
        guide.Updatedat = DateTime.UtcNow;

        _sizeGuideRepository.Update(guide);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateSizeGuideResponse(guide.Guideid));
    }
}
