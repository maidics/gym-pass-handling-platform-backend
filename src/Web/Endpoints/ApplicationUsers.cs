using Fitpass.Application.ApplicationUsers.Commands;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints.ApplicationUsers;

public class ApplicationUsers : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterUser, "Register");

        groupBuilder.MapPost(LogInUser, "LogIn");

        groupBuilder.MapPost(RegisterGymAdministratorUser, "Register/GymAdministrator").RequireAuthorization();

        //TODO: create endpoint for Register/GymStaff
    }

    public async Task<Ok<string>> RegisterUser(ISender sender, [FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<string>> LogInUser(ISender sender, [FromBody] LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<Result>> RegisterGymAdministratorUser(ISender sender, [AsParameters] RegisterGymAdministratorUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}
