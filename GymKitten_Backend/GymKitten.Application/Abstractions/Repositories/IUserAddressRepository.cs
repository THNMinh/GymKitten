using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IUserAddressRepository
{
    Task<IEnumerable<Useraddress>> SearchUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Useraddress?> GetByIdAsync(Guid addressId, CancellationToken cancellationToken = default);

    Task ClearDefaultAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Useraddress address, CancellationToken cancellationToken = default);

    void Update(Useraddress address);

    void Remove(Useraddress address);
}
