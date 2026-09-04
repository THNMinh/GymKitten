using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Identity.Addresses.Commands.DeleteAddress;

public sealed record DeleteAddressCommand(Guid AddressId) : ICommand<Result>;
