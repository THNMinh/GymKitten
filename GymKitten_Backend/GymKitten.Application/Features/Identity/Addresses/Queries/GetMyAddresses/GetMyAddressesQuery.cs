using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Identity.Addresses.Queries.GetMyAddresses;

public sealed record GetMyAddressesQuery : IQuery<Result<GetMyAddressesResponse>>;
