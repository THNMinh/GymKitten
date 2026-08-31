using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface IOrderTrackingRepository
{
    Task<List<Ordertrackinghistory>> GetTrackingHistoryByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Ordertrackinghistory trackingHistory,
        CancellationToken cancellationToken = default);
}
