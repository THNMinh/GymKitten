using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.UpdateAddress;

public sealed class UpdateAddressCommandHandler
    : ICommandHandler<UpdateAddressCommand, Result<UpdateAddressResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAddressCommandHandler(
        IUserContext userContext,
        IUserAddressRepository userAddressRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userAddressRepository = userAddressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateAddressResponse>> Handle(
        UpdateAddressCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<UpdateAddressResponse>(UserErrors.Unauthorized);
        }

        var address = await _userAddressRepository.GetByIdAsync(request.AddressId, cancellationToken);
        if (address is null)
        {
            return Result.Failure<UpdateAddressResponse>(UserAddressErrors.NotFound);
        }

        if (address.Userid != _userContext.UserId.Value)
        {
            return Result.Failure<UpdateAddressResponse>(UserAddressErrors.Forbidden);
        }

        if (request.IsDefault && !address.Isdefault)
        {
            await _userAddressRepository.ClearDefaultAddressesAsync(_userContext.UserId.Value, cancellationToken);
        }

        address.Receivername = request.ReceiverName.Trim();
        address.Phonenumber = request.PhoneNumber.Trim();
        address.Addressline1 = request.AddressLine1.Trim();
        address.Ward = request.Ward.Trim();
        address.District = request.District.Trim();
        address.City = request.City.Trim();
        address.Isdefault = request.IsDefault;
        address.Addresstype = string.IsNullOrWhiteSpace(request.AddressType) ? address.Addresstype : request.AddressType.Trim();
        address.Updatedat = DateTime.UtcNow;

        _userAddressRepository.Update(address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateAddressResponse(address.Addressid));
    }
}
