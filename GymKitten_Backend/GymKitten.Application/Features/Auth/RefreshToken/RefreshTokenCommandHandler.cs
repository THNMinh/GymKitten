using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Auth.Login;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;

    public RefreshTokenCommandHandler(
        IApplicationDbContext dbContext,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find the refresh token in DB
        var storedToken = await _dbContext.Refreshtokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null)
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidRefreshToken);
        }

        // 2. Check if revoked or already used
        if (storedToken.Isrevoked || storedToken.Isused)
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidRefreshToken);
        }

        // 3. Check if expired
        if (storedToken.Expirydate < DateTime.UtcNow)
        {
            return Result.Failure<LoginResponse>(AuthErrors.ExpiredRefreshToken);
        }

        // 4. Check if user is active
        if (!storedToken.User.Isactive)
        {
            return Result.Failure<LoginResponse>(AuthErrors.AccountInactive);
        }

        // 5. Mark old token as used (Token Rotation)
        storedToken.Isused = true;
        storedToken.Updatedat = DateTime.UtcNow;

        // 6. Generate new access token
        var (newAccessToken, newJwtId) = _jwtProvider.GenerateAccessToken(storedToken.User);

        // 7. Generate new refresh token
        var newRefreshTokenString = _jwtProvider.GenerateRefreshToken();

        var newRefreshToken = new Refreshtoken
        {
            Refreshtokenid = Guid.NewGuid(),
            Userid = storedToken.Userid,
            Token = newRefreshTokenString,
            Jwtid = newJwtId,
            Isused = false,
            Isrevoked = false,
            Expirydate = DateTime.UtcNow.AddDays(7),
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        // 8. Save to DB
        _dbContext.Refreshtokens.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Return new tokens
        return Result.Success(new LoginResponse(newAccessToken, newRefreshTokenString));
    }
}
