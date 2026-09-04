using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.CreateAddress;

public sealed record CreateAddressCommand(
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string Ward,
    string District,
    string City,
    bool IsDefault,
    string AddressType) : ICommand<Result<CreateAddressResponse>>;

public sealed record CreateAddressResponse(Guid AddressId);
