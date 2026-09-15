using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Admin.Users.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Users.Queries.GetAdminUserById;

public sealed class GetAdminUserByIdQueryHandler
    : IQueryHandler<GetAdminUserByIdQuery, Result<AdminUserDetailsResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;

    public GetAdminUserByIdQueryHandler(
        IUserContext userContext,
        IUserRepository userRepository)
    {
        _userContext = userContext;
        _userRepository = userRepository;
    }

    public async Task<Result<AdminUserDetailsResponse>> Handle(
        GetAdminUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AdminUserDetailsResponse>(UserErrors.Forbidden);
        }

        var user = await _userRepository.GetByIdWithDetailsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AdminUserDetailsResponse>(UserErrors.NotFound);
        }

        var addressDtos = user.Useraddresses
            .Where(a => a.Deletedat == null)
            .Select(a => new AdminUserAddressDto(
                a.Addressid,
                a.Receivername,
                a.Phonenumber,
                a.Addressline1,
                a.Ward,
                a.District,
                a.City,
                a.Isdefault,
                a.Addresstype))
            .ToList();

        var recentOrderDtos = user.Orders
            .OrderByDescending(o => o.Createdat)
            .Take(10)
            .Select(o => new AdminUserOrderSummaryDto(
                o.Orderid,
                o.Ordercode,
                o.Totalamount,
                o.Currentstatus,
                o.Paymentmethod,
                o.Paymentstatus,
                o.Createdat))
            .ToList();

        var totalOrders = user.Orders.Count;
        var totalSpent = user.Orders
            .Where(o => !string.Equals(o.Currentstatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            .Sum(o => o.Totalamount);

        var response = new AdminUserDetailsResponse(
            user.Userid,
            user.Email,
            user.Fullname,
            user.Phone,
            user.Role,
            user.Isemailverified,
            user.Isactive,
            user.Avatarurl,
            user.Createdat,
            user.Updatedat,
            addressDtos,
            recentOrderDtos,
            totalOrders,
            totalSpent);

        return Result.Success(response);
    }
}
