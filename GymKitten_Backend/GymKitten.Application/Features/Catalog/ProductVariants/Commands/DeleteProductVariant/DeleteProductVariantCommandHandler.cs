using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.DeleteProductVariant;

public sealed class DeleteProductVariantCommandHandler
    : ICommandHandler<DeleteProductVariantCommand, Result>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductVariantCommandHandler(
        IProductVariantRepository productVariantRepository,
        IUnitOfWork unitOfWork)
    {
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        var variant = await _productVariantRepository.GetByIdAsync(request.VariantId, cancellationToken);
        if (variant is null)
        {
            return Result.Failure(ProductVariantErrors.NotFound);
        }

        _productVariantRepository.Remove(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
