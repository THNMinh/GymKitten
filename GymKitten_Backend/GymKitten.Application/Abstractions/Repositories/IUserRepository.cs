using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    void Update(User user);

    void Delete(User user);

    Task<(List<User> Users, int TotalCount)> SearchUsersAsync(
        string? searchTerm,
        string? role,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
}
