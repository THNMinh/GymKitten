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
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, Result<AdminUserItemDto>>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public CreateUserCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ISystemLogService systemLogService)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<Result<AdminUserItemDto>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        //if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        //{
        //    return Result.Failure<AdminUserItemDto>(UserErrors.Forbidden);
        //}

        var normalizedRole = request.Role?.Trim();
        if (!string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(normalizedRole, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AdminUserItemDto>(UserErrors.InvalidRole);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (exists)
        {
            return Result.Failure<AdminUserItemDto>(UserErrors.EmailAlreadyExists);
        }

        var user = new User
        {
            Userid = Guid.NewGuid(),
            Email = normalizedEmail,
            Passwordhash = _passwordHasher.Hash(request.Password),
            Fullname = request.FullName?.Trim(),
            Phone = request.Phone?.Trim(),
            Role = string.Equals(normalizedRole, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Customer",
            Isactive = request.IsActive,
            Isemailverified = true,
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _systemLogService.LogAsync(
            "CreateUser",
            $"Admin {_userContext.Email} created user {user.Email} (Role: {user.Role})",
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
