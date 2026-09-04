namespace GymKitten.Application.Features.Identity.Addresses.Queries.GetMyAddresses;

public sealed record GetMyAddressesResponse(List<UserAddressDto> Items);

public sealed record UserAddressDto(
    Guid AddressId,
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string Ward,
    string District,
    string City,
    bool IsDefault,
    string AddressType,
    DateTime CreatedAt);
