using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly GymkittenContext _context;

    public UserRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Userid == userId, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task<(List<User> Users, int TotalCount)> SearchUsersAsync(
        string? searchTerm,
        string? role,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.Deletedat == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                (u.Fullname != null && u.Fullname.ToLower().Contains(term)) ||
                (u.Phone != null && u.Phone.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(role) && !role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var roleLower = role.Trim().ToLower();
            query = query.Where(u => u.Role.ToLower() == roleLower);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.Isactive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 10 : pageSize;

        var users = await query
            .OrderByDescending(u => u.Createdat)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    public async Task<User?> GetByIdWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Useraddresses)
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Userid == userId && u.Deletedat == null, cancellationToken);
    }
}
