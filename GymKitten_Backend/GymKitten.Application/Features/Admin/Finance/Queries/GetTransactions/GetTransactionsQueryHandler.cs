using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Finance.Queries.GetTransactions;

public sealed class GetTransactionsQueryHandler
    : IQueryHandler<GetTransactionsQuery, Result<GetTransactionsResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;

    public GetTransactionsQueryHandler(
        IUserContext userContext,
        IPaymentTransactionRepository paymentTransactionRepository)
    {
        _userContext = userContext;
        _paymentTransactionRepository = paymentTransactionRepository;
    }

    public async Task<Result<GetTransactionsResponse>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetTransactionsResponse>(UserErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var (items, totalCount) = await _paymentTransactionRepository.GetTransactionsPagedAsync(
            request.StartDate,
            request.EndDate,
            request.Status,
            request.Gateway,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new GetTransactionsResponse(
            items,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
