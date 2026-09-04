using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Identity.Addresses.Commands.CreateAddress;
using GymKitten.Application.Features.Identity.Addresses.Commands.DeleteAddress;
using GymKitten.Application.Features.Identity.Addresses.Commands.SetDefaultAddress;
using GymKitten.Application.Features.Identity.Addresses.Commands.UpdateAddress;
using GymKitten.Application.Features.Identity.Addresses.Queries.GetMyAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/user/addresses")]
[Authorize]
public class UserAddressesController : ControllerBase
{
    private readonly ISender _sender;

    public UserAddressesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetMyAddresses(CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetMyAddressesQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    public async Task<IResult> CreateAddress(
        [FromBody] CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateAddressCommand(
            request.ReceiverName,
            request.PhoneNumber,
            request.AddressLine1,
            request.Ward,
            request.District,
            request.City,
            request.IsDefault,
            request.AddressType);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{addressId:guid}")]
    public async Task<IResult> UpdateAddress(
        Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateAddressCommand(
            addressId,
            request.ReceiverName,
            request.PhoneNumber,
            request.AddressLine1,
            request.Ward,
            request.District,
            request.City,
            request.IsDefault,
            request.AddressType);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{addressId:guid}/set-default")]
    public async Task<IResult> SetDefaultAddress(
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new SetDefaultAddressCommand(addressId), cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{addressId:guid}")]
    public async Task<IResult> DeleteAddress(
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteAddressCommand(addressId), cancellationToken);
        return result.MatchOk();
    }
}
