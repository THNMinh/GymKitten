using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;

public sealed record GetTransactionsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Status = null,
    string? Gateway = null,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<GetTransactionsResponse>>;
