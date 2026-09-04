using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.SetDefaultAddress;

public sealed class SetDefaultAddressCommandHandler
    : ICommandHandler<SetDefaultAddressCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetDefaultAddressCommandHandler(
        IUserContext userContext,
        IUserAddressRepository userAddressRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userAddressRepository = userAddressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetDefaultAddressCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        var address = await _userAddressRepository.GetByIdAsync(request.AddressId, cancellationToken);
        if (address is null)
        {
            return Result.Failure(UserAddressErrors.NotFound);
        }

        if (address.Userid != _userContext.UserId.Value)
        {
            return Result.Failure(UserAddressErrors.Forbidden);
        }

        await _userAddressRepository.ClearDefaultAddressesAsync(_userContext.UserId.Value, cancellationToken);

        address.Isdefault = true;
        address.Updatedat = DateTime.UtcNow;

        _userAddressRepository.Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
