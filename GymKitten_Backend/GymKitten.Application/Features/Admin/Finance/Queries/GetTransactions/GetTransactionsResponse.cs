namespace GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;

public sealed record GetTransactionsResponse(
    List<TransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
