using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class UserAddressRepository : IUserAddressRepository
{
    private readonly GymkittenContext _context;

    public UserAddressRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Useraddress>> SearchUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Useraddresses
            .AsNoTracking()
            .Where(a => a.Userid == userId && a.Deletedat == null)
            .OrderByDescending(a => a.Isdefault)
            .ThenByDescending(a => a.Createdat)
            .ToListAsync(cancellationToken);
    }

    public async Task<Useraddress?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        return await _context.Useraddresses
            .FirstOrDefaultAsync(a => a.Addressid == addressId && a.Deletedat == null, cancellationToken);
    }

    public async Task ClearDefaultAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _context.Useraddresses
            .Where(a => a.Userid == userId && a.Isdefault && a.Deletedat == null)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Isdefault, false), cancellationToken);
    }

    public async Task AddAsync(Useraddress address, CancellationToken cancellationToken = default)
    {
        await _context.Useraddresses.AddAsync(address, cancellationToken);
    }

    public void Update(Useraddress address)
    {
        _context.Useraddresses.Update(address);
    }

    public void Remove(Useraddress address)
    {
        _context.Useraddresses.Remove(address);
    }
}
