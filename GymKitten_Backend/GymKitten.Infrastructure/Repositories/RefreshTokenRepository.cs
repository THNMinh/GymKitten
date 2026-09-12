using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly GymkittenContext _context;

    public RefreshTokenRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<Refreshtoken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Refreshtokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task AddAsync(Refreshtoken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.Refreshtokens.AddAsync(refreshToken, cancellationToken);
    }

    public void Update(Refreshtoken refreshToken)
    {
        _context.Refreshtokens.Update(refreshToken);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _context.Refreshtokens
            .Where(rt => rt.Userid == userId && !rt.Isrevoked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.Isrevoked, true)
                .SetProperty(rt => rt.Updatedat, DateTime.UtcNow),
                cancellationToken);
    }
}
