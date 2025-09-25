using Fitpass.Application.NonRegisteredUsers.Commands;
using Fitpass.Application.NonRegisteredUsers.Queries;
using Fitpass.Web.Infrastructure;
using FitPass.Application.Common.Models;
using FitPass.Application.NonRegisteredUsers.Commands;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Application.NonRegisteredUsers.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class NonRegisteredUsers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateNonRegisteredUser, "RegisterByGymManagement").RequireAuthorization();

        groupBuilder.MapGet(GetNonRegisteredUser, "{nonRegisteredUserId}").RequireAuthorization();

        groupBuilder.MapGet(GetAllMyNonRegisteredUsers, "All/My").RequireAuthorization();

        groupBuilder.MapPost(AddUserGymMembershipToNonRegisteredUser, "{nonRegisteredUserId}/UserGymMembership").RequireAuthorization();

        groupBuilder.MapPost(BuyPassForNonRegisteredUser, "{nonRegisteredUserId}/BuyPass/{gymPassProductId}").RequireAuthorization();

        groupBuilder.MapPost(RegisterNonRegisteredUser, "/Register").AllowAnonymousOnly();
    }

    public async Task<Ok<NonRegisteredUserDto>> CreateNonRegisteredUser(ISender sender, CreateNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<NonRegisteredUserDto>> GetNonRegisteredUser(ISender sender, string nonRegisteredUserId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNonRegisteredUserQuery(nonRegisteredUserId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<NonRegisteredUserDto>>> GetAllMyNonRegisteredUsers(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllMyNonRegisteredUsersQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<NonRegisteredUserDto>> AddUserGymMembershipToNonRegisteredUser(ISender sender, string nonRegisteredUserId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddUserGymMembershipToNonRegisteredUserCommand(nonRegisteredUserId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok> BuyPassForNonRegisteredUser(ISender sender, string nonRegisteredUserId, string gymPassProductId, CancellationToken cancellationToken)
    {
        await sender.Send(new BuyPassForNonRegisteredUserCommand(nonRegisteredUserId, gymPassProductId), cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok> RegisterNonRegisteredUser(ISender sender, [FromBody] RegisterNonRegisteredUserCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.Ok();
    }
}
