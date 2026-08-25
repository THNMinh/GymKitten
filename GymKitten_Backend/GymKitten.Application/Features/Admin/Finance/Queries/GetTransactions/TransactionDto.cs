namespace GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;

public sealed record TransactionDto(
    Guid TransactionId,
    string OrderCode,
    string UserEmail,
    string Gateway,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    DateTime? PaymentDate);
