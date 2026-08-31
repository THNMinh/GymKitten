namespace GymKitten.API.Requests;

public sealed record UpdateOrderStatusRequest(
    string Status,
    string? Title,
    string? Description,
    string? Location);
