using System;
using System.Threading;
using System.Threading.Tasks;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Application.Features.Admin.Users.DTOs;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler
    : ICommandHandler<UpdateUserCommand, Result<AdminUserItemDto>>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public UpdateUserCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ISystemLogService systemLogService)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<Result<AdminUserItemDto>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AdminUserItemDto>(UserErrors.Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AdminUserItemDto>(UserErrors.NotFound);
        }

        // Prevent admin from deactivating themselves
        if (request.IsActive.HasValue && !request.IsActive.Value && _userContext.UserId == user.Userid)
        {
            return Result.Failure<AdminUserItemDto>(UserErrors.CannotDeleteSelf);
        }

        // Validate and apply Role if changed
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var normalizedRole = request.Role.Trim();
            if (!string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedRole, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<AdminUserItemDto>(UserErrors.InvalidRole);
            }

            // Prevent admin from demoting themselves
            if (!string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) && _userContext.UserId == user.Userid)
            {
                return Result.Failure<AdminUserItemDto>(UserErrors.CannotDeleteSelf);
            }

            user.Role = string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer";
        }

        if (request.FullName != null)
        {
            user.Fullname = request.FullName.Trim();
        }

        if (request.Phone != null)
        {
            user.Phone = request.Phone.Trim();
        }

        if (request.IsActive.HasValue)
        {
            user.Isactive = request.IsActive.Value;
        }

        if (request.IsEmailVerified.HasValue)
        {
            user.Isemailverified = request.IsEmailVerified.Value;
        }

        user.Updatedat = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _systemLogService.LogAsync(
            "UpdateUser",
            $"Admin {_userContext.Email} updated user {user.Email} (ID: {user.Userid})",
            "Information",
            _userContext.UserId,
            cancellationToken);

        var dto = new AdminUserItemDto(
            user.Userid,
            user.Email,
            user.Fullname,
            user.Phone,
            user.Role,
            user.Isemailverified,
            user.Isactive,
            user.Avatarurl,
            user.Createdat,
            user.Updatedat);

        return Result.Success(dto);
    }
}
