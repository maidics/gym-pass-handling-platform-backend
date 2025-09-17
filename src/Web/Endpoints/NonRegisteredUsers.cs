
using Fitpass.Application.NonRegisteredUsers.Commands;
using Fitpass.Application.NonRegisteredUsers.Queries;
using FitPass.Application.Common.Models;
using FitPass.Application.NonRegisteredUsers.Commands;
using FitPass.Application.NonRegisteredUsers.DTOs;
using FitPass.Application.NonRegisteredUsers.Queries;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class NonRegisteredUsers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateNonRegisteredUser).RequireAuthorization();

        groupBuilder.MapGet(GetNonRegisteredUser).RequireAuthorization();

        groupBuilder.MapGet(GetAllMyNonRegisteredUsers).RequireAuthorization();

        groupBuilder.MapPut(AddUserGymMembershipToNonRegisteredUser, "{id}").RequireAuthorization();

        groupBuilder.MapPut(BuyPassForNonRegisteredUser, "{id}").RequireAuthorization();
    }

    public async Task<Ok<NonRegisteredUserDto>> CreateNonRegisteredUser(ISender sender, CreateNonRegisteredUserCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<NonRegisteredUserDto>, NotFound>> GetNonRegisteredUser(ISender sender, [AsParameters] GetNonRegisteredUserQuery query)
    {
        var result = await sender.Send(query);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<NonRegisteredUserDto>>> GetAllMyNonRegisteredUsers(ISender sender, GetAllMyNonRegisteredUsersQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<NonRegisteredUserDto>, NotFound, BadRequest>> AddUserGymMembershipToNonRegisteredUser(ISender sender, string id, [AsParameters] AddUserGymMembershipToNonRegisteredUserCommand command)
    {
        if (id != command.NonRegisteredUserId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<Result>, BadRequest>> BuyPassForNonRegisteredUser(ISender sender, string id, [AsParameters] BuyPassForNonRegisteredUserCommand command)
    {
        if (id != command.NonRegisteredUserId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}
