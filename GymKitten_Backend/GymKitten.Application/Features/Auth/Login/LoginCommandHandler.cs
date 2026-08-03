using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Auth.Login;

public sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        // 2. Check if account is active
        if (!user.Isactive)
        {
            return Result.Failure<LoginResponse>(AuthErrors.AccountInactive);
        }

        // 3. Verify password
        if (string.IsNullOrEmpty(user.Passwordhash) ||
            !_passwordHasher.Verify(request.Password, user.Passwordhash))
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        // 4. Generate access token (with JwtId)
        var (accessToken, jwtId) = _jwtProvider.GenerateAccessToken(user);

        // 5. Generate refresh token string
        var refreshTokenString = _jwtProvider.GenerateRefreshToken();

        // 6. Save refresh token to DB via Repository
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

        // 7. Return response
        return Result.Success(new LoginResponse(accessToken, refreshTokenString));
    }
}
