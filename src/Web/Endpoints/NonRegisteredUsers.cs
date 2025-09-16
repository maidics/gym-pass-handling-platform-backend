
using FitPass.Application.NonRegisteredUsers.Commands;
using FitPass.Application.NonRegisteredUsers.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class NonRegisteredUsers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateNonRegisteredUser).RequireAuthorization();
    }

    public async Task<Ok<NonRegisteredUserDto>> CreateNonRegisteredUser(ISender sender, CreateNonRegisteredUserCommand command)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}
