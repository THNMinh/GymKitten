using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.SetDefaultAddress;

public sealed record SetDefaultAddressCommand(Guid AddressId) : ICommand<Result>;
