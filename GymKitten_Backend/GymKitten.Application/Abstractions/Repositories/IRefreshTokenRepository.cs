using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IRefreshTokenRepository
{
    Task<Refreshtoken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default);

    Task AddAsync(Refreshtoken refreshToken, CancellationToken cancellationToken = default);

    void Update(Refreshtoken refreshToken);

    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
