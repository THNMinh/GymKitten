using System.Security.Cryptography;
using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;
using GymKitten.Domain.Events;

namespace GymKitten.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        if (!user.Isactive)
        {
            return Result.Failure(AuthErrors.AccountInactive);
        }

        // 1. Generate secure random 10-character password (e.g. Gk#8xK2p@9)
        var newTempPassword = GenerateSecurePassword(10);

        // 2. Hash and update user's password
        user.Passwordhash = _passwordHasher.Hash(newTempPassword);
        user.Updatedat = DateTime.UtcNow;
        _userRepository.Update(user);

        // 3. Raise domain event so Hangfire sends the new temporary password to email
        user.RaiseDomainEvent(new ForgotPasswordDomainEvent(user.Email, newTempPassword));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string GenerateSecurePassword(int length = 10)
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%*";
        const string allChars = upper + lower + digits + special;

        var chars = new char[length];
        chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (int i = 4; i < length; i++)
        {
            chars[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
        }

        // Shuffle
        for (int i = length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}
