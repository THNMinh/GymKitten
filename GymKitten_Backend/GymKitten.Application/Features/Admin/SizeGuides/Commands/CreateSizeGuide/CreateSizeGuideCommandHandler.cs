using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.SizeGuides.Commands.CreateSizeGuide;

public sealed class CreateSizeGuideCommandHandler
    : ICommandHandler<CreateSizeGuideCommand, Result<CreateSizeGuideResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IProductRepository _productRepository;
    private readonly ISizeGuideRepository _sizeGuideRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSizeGuideCommandHandler(
        IUserContext userContext,
        IProductRepository productRepository,
        ISizeGuideRepository sizeGuideRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _productRepository = productRepository;
        _sizeGuideRepository = sizeGuideRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateSizeGuideResponse>> Handle(
        CreateSizeGuideCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<CreateSizeGuideResponse>(UserErrors.Forbidden);
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<CreateSizeGuideResponse>(ProductErrors.NotFound);
        }

        var now = DateTime.UtcNow;
        var guide = new Sizeguide
        {
            Guideid = Guid.NewGuid(),
            Productid = request.ProductId,
            Size = request.Size.Trim().ToUpper(),
            Chestcm = request.ChestCm?.Trim(),
            Waistcm = request.WaistCm?.Trim(),
            Hipscm = request.HipsCm?.Trim(),
            Heightrangecm = request.HeightRangeCm?.Trim(),
            Createdat = now,
            Updatedat = now
        };

        await _sizeGuideRepository.AddAsync(guide, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateSizeGuideResponse(guide.Guideid));
    }
}
