namespace GymKitten.API.Requests;

public sealed record CreateAddressRequest(
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string Ward,
    string District,
    string City,
    bool IsDefault = false,
    string AddressType = "Home");

public sealed record UpdateAddressRequest(
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string Ward,
    string District,
    string City,
    bool IsDefault = false,
    string AddressType = "Home");
