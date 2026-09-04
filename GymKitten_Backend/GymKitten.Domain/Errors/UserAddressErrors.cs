using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class UserAddressErrors
{
    public static readonly Error NotFound = new(
        "UserAddress.NotFound",
        "The requested shipping address was not found.");

    public static readonly Error Forbidden = new(
        "UserAddress.Forbidden",
        "You do not have permission to access or modify this address.");

    public static readonly Error InvalidPhoneNumber = new(
        "UserAddress.InvalidPhoneNumber",
        "Please provide a valid phone number for shipping.");
}
