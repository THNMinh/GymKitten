using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.CreateAddress;

public sealed class CreateAddressCommandHandler
    : ICommandHandler<CreateAddressCommand, Result<CreateAddressResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAddressCommandHandler(
        IUserContext userContext,
        IUserAddressRepository userAddressRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userAddressRepository = userAddressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateAddressResponse>> Handle(
        CreateAddressCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<CreateAddressResponse>(UserErrors.Unauthorized);
        }

        var userId = _userContext.UserId.Value;
        var existingAddresses = await _userAddressRepository.SearchUserAddressesAsync(userId, cancellationToken);
        var isFirstAddress = !existingAddresses.Any();

        bool isDefaultToSet = request.IsDefault || isFirstAddress;

        if (isDefaultToSet)
        {
            await _userAddressRepository.ClearDefaultAddressesAsync(userId, cancellationToken);
        }

        var now = DateTime.UtcNow;
        var address = new Useraddress
        {
            Addressid = Guid.NewGuid(),
            Userid = userId,
            Receivername = request.ReceiverName.Trim(),
            Phonenumber = request.PhoneNumber.Trim(),
            Addressline1 = request.AddressLine1.Trim(),
            Ward = request.Ward.Trim(),
            District = request.District.Trim(),
            City = request.City.Trim(),
            Isdefault = isDefaultToSet,
            Addresstype = string.IsNullOrWhiteSpace(request.AddressType) ? "Home" : request.AddressType.Trim(),
            Createdat = now,
            Updatedat = now
        };

        await _userAddressRepository.AddAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateAddressResponse(address.Addressid));
    }
}
