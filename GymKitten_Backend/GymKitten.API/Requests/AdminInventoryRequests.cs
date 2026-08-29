namespace GymKitten.API.Requests;

public sealed record RestockRequest(
    Guid VariantId,
    int Quantity,
    string? ReferenceId);

public sealed record AdjustStockRequest(
    Guid VariantId,
    int NewQuantity,
    string? Note);
