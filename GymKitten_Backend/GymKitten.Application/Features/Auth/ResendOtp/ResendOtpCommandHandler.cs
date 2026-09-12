using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;
using GymKitten.Domain.Events;

namespace GymKitten.Application.Features.Auth.ResendOtp;

public sealed class ResendOtpCommandHandler : ICommandHandler<ResendOtpCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpGenerator _otpGenerator;
    private readonly IRedisOtpStore _otpStore;
    private readonly IUnitOfWork _unitOfWork;

    public ResendOtpCommandHandler(
        IUserRepository userRepository,
        IOtpGenerator otpGenerator,
        IRedisOtpStore otpStore,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _otpGenerator = otpGenerator;
        _otpStore = otpStore;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ResendOtpCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        if (user.Isemailverified)
        {
            return Result.Failure(AuthErrors.UserAlreadyVerified);
        }

        // 1. Generate new 6-digit OTP
        var newOtp = _otpGenerator.Generate6Digits();

        // 2. Store in Redis, overwriting old key and resetting TTL to 5 minutes
        await _otpStore.StoreAsync(
            user.Userid,
            newOtp,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        // 3. Raise domain event for Hangfire email dispatcher
        user.RaiseDomainEvent(new ResendOtpDomainEvent(
            user.Userid,
            user.Email,
            newOtp));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
