using FitPass.Application.Common.Interfaces;
using FitPass.Application.Gyms.Commands;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Gyms.Queries;
using FitPass.Application.Requests.Commands.Fulfill;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitPass.Web.Endpoints;

public class Gyms : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateMyGymProfile, "My/Profile").RequireAuthorization();

        groupBuilder.MapGet(GetAllGyms);

        groupBuilder.MapGet(GetGymById, "{gymId}/Details").RequireAuthorization();

        //groupBuilder.MapGet(GetNewGymsThisMonth, "NewThisMonth").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymStatus, "{gymId}/Status").RequireAuthorization();
        
        groupBuilder.MapPost(RegisterGymFromRequest, "Register/FromRequest").RequireAuthorization();

        //groupBuilder.MapPost(RegisterGym, "Register").RequireAuthorization();

        groupBuilder.MapPut(UpdateMyGymStatus, "My/Status").RequireAuthorization();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateMyGymProfile(
        ISender sender, [FromBody] UpdateMyGymProfileCommand command)
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Ok<List<GymDto>>> GetAllGyms(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<GymDto>, ProblemHttpResult>> GetGymById(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymByIdQuery(gymId), cancellationToken);

        return result.ToTypedResult();
    }

    /*
    public async Task<Ok<List<GymDto>>> GetNewGymsThisMonth(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNewGymsThisMonthQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }
    */

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateGymStatus(
        ISender sender, string gymId, [FromBody] UpdateGymStatusCommand command)
    {
        if (gymId != command.GymId)
        {
            return TypedResults.Problem(new ProblemDetails { Status = StatusCodes.Status400BadRequest });
        }
        
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateMyGymStatus(
        ISender sender, [FromBody] UpdateMyGymStatusCommand command)
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }
    
    public async Task<Results<Ok<GymDto>, ProblemHttpResult>> RegisterGymFromRequest(
        ISender sender, string requestId)
    {
        var result = await sender.Send(new RegisterGymFromRequestCommand(requestId), CancellationToken.None);

        return result.ToTypedResult();
    }
}
