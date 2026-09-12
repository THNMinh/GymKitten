using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (_userContext.UserId is null)
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(_userContext.UserId.Value, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        if (string.IsNullOrEmpty(user.Passwordhash) ||
            !_passwordHasher.Verify(request.CurrentPassword, user.Passwordhash))
        {
            return Result.Failure(AuthErrors.WrongPassword);
        }

        user.Passwordhash = _passwordHasher.Hash(request.NewPassword);
        user.Updatedat = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
