using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<Order?> GetByIdWithDetailsAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<(List<Order> Items, int TotalCount)> GetMyOrdersPagedAsync(
        Guid userId,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(List<Order> Items, int TotalCount)> GetAdminOrdersPagedAsync(
        string? orderCode,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    void Update(Order order);
}
