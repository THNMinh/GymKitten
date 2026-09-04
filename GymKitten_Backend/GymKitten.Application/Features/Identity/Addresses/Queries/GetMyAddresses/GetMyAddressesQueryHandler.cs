using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Identity.Addresses.Queries.GetMyAddresses;

public sealed class GetMyAddressesQueryHandler
    : IQueryHandler<GetMyAddressesQuery, Result<GetMyAddressesResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IUserAddressRepository _userAddressRepository;

    public GetMyAddressesQueryHandler(
        IUserContext userContext,
        IUserAddressRepository userAddressRepository)
    {
        _userContext = userContext;
        _userAddressRepository = userAddressRepository;
    }

    public async Task<Result<GetMyAddressesResponse>> Handle(
        GetMyAddressesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<GetMyAddressesResponse>(UserErrors.Unauthorized);
        }

        var addresses = await _userAddressRepository.SearchUserAddressesAsync(_userContext.UserId.Value, cancellationToken);

        var dtos = addresses.Select(a => new UserAddressDto(
            a.Addressid,
            a.Receivername,
            a.Phonenumber,
            a.Addressline1,
            a.Ward,
            a.District,
            a.City,
            a.Isdefault,
            a.Addresstype,
            a.Createdat
        )).ToList();

        return Result.Success(new GetMyAddressesResponse(dtos));
    }
}
