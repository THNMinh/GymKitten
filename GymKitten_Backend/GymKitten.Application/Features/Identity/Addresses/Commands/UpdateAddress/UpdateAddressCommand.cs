using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.UpdateAddress;

public sealed record UpdateAddressCommand(
    Guid AddressId,
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string Ward,
    string District,
    string City,
    bool IsDefault,
    string AddressType) : ICommand<Result<UpdateAddressResponse>>;

public sealed record UpdateAddressResponse(Guid AddressId);
