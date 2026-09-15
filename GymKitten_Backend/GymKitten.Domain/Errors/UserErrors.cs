using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new(
        "User.NotFound",
        "The specified user was not found.");

    public static readonly Error Unauthorized = new(
        "User.Unauthorized",
        "Authentication is required to access this resource.");

    public static readonly Error Forbidden = new(
        "User.Forbidden",
        "You do not have administrative privileges to perform this operation.");

    public static readonly Error AccessDenied = new(
        "User.AccessDenied",
        "Access denied. You do not have permission to perform this action.");

    public static readonly Error EmailAlreadyExists = new(
        "User.EmailAlreadyExists",
        "Email này đã được sử dụng bởi tài khoản khác trong hệ thống.");

    public static readonly Error CannotDeleteSelf = new(
        "User.CannotDeleteSelf",
        "Bạn không thể tự xóa hoặc vô hiệu hóa tài khoản của chính mình.");

    public static readonly Error CannotDeleteAdmin = new(
        "User.CannotDeleteAdmin",
        "Không thể xóa tài khoản Quản trị viên cấp cao của hệ thống.");

    public static readonly Error InvalidRole = new(
        "User.InvalidRole",
        "Vai trò người dùng không hợp lệ. Chỉ chấp nhận 'Customer' hoặc 'Admin'.");
}
