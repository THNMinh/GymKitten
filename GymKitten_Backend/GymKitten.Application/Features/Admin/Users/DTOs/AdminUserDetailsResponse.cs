using System;
using System.Collections.Generic;

namespace GymKitten.Application.Features.Admin.Users.DTOs;

public record AdminUserAddressDto(
    Guid AddressId,
    string ReceiverName,
    string PhoneNumber,
    string AddressLine1,
    string Ward,
    string District,
    string City,
    bool IsDefault,
    string AddressType);

public record AdminUserOrderSummaryDto(
    Guid OrderId,
    string OrderCode,
    decimal TotalAmount,
    string CurrentStatus,
    string PaymentMethod,
    string PaymentStatus,
    DateTime CreatedAt);

public record AdminUserDetailsResponse(
    Guid UserId,
    string Email,
    string? FullName,
    string? Phone,
    string Role,
    bool IsEmailVerified,
    bool IsActive,
    string? AvatarUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<AdminUserAddressDto> Addresses,
    List<AdminUserOrderSummaryDto> RecentOrders,
    int TotalOrders,
    decimal TotalSpent);
