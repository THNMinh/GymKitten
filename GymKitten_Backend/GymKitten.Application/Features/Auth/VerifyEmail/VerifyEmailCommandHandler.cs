using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailCommandHandler
    : ICommandHandler<VerifyEmailCommand, Result<VerifyEmailResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRedisOtpStore _otpStore;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IRedisOtpStore otpStore,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtProvider jwtProvider,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _otpStore = otpStore;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VerifyEmailResponse>> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (user is null)
        {
            return Result.Failure<VerifyEmailResponse>(AuthErrors.UserNotFound);
        }

        if (user.Isemailverified)
        {
            return Result.Failure<VerifyEmailResponse>(AuthErrors.UserAlreadyVerified);
        }

        // 2. Fetch OTP from Redis store
        var storedOtp = await _otpStore.GetAsync(user.Userid, cancellationToken);
        if (string.IsNullOrEmpty(storedOtp))
        {
            return Result.Failure<VerifyEmailResponse>(AuthErrors.OtpExpired);
        }

        // 3. Verify OTP code
        if (!string.Equals(storedOtp.Trim(), request.OtpCode.Trim(), StringComparison.Ordinal))
        {
            return Result.Failure<VerifyEmailResponse>(AuthErrors.WrongOtp);
        }

        // 4. Remove OTP from Redis immediately to prevent replay attacks
        await _otpStore.RemoveAsync(user.Userid, cancellationToken);

        // 5. Update user state
        user.Isemailverified = true;
        user.Updatedat = DateTime.UtcNow;
        _userRepository.Update(user);

        // 6. Generate authenticated tokens so user is immediately logged in
        var (accessToken, jwtId) = _jwtProvider.GenerateAccessToken(user);
        var refreshTokenString = _jwtProvider.GenerateRefreshToken();

        var refreshToken = new Refreshtoken
        {
            Refreshtokenid = Guid.NewGuid(),
            Userid = user.Userid,
            Token = refreshTokenString,
            Jwtid = jwtId,
            Isused = false,
            Isrevoked = false,
            Expirydate = DateTime.UtcNow.AddDays(7),
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new VerifyEmailResponse(accessToken, refreshTokenString));
    }
}
