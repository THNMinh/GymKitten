using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Order.Queries.GetOrderTracking;

public sealed record GetOrderTrackingQuery(Guid OrderId) : IQuery<Result<List<OrderTrackingDto>>>;

public sealed record OrderTrackingDto(
    Guid TrackingId,
    Guid OrderId,
    string Status,
    string Title,
    string? Description,
    string? Location,
    DateTime Timestamp,
    DateTime CreatedAt);
