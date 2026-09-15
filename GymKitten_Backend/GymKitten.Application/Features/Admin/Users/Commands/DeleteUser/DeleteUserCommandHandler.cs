using System;
using System.Threading;
using System.Threading.Tasks;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public DeleteUserCommandHandler(
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

    public async Task<Result> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        if (_userContext.UserId == user.Userid)
        {
            return Result.Failure(UserErrors.CannotDeleteSelf);
        }

        user.Isactive = false;
        user.Deletedat = DateTime.UtcNow;
        user.Updatedat = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _systemLogService.LogAsync(
            "DeleteUser",
            $"Admin {_userContext.Email} deactivated/deleted user {user.Email} (ID: {user.Userid})",
            "Warning",
            _userContext.UserId,
            cancellationToken);

        return Result.Success();
    }
}
