using FitPass.Application.Gyms.Commands;
using FitPass.Application.Gyms.DTOs;
using FitPass.Application.Gyms.Queries;
using FitPass.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class Gyms : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateMyGymProfile, "My/Profile").RequireAuthorization();

        groupBuilder.MapGet(GetAllGyms);

        groupBuilder.MapGet(GetGymDetails, "{gymId}/Details").RequireAuthorization();

        groupBuilder.MapGet(GetNewGymsThisMonth, "NewThisMonth").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymStatus, "{gymId}/Status").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymDetails, "My/Details").RequireAuthorization();

        groupBuilder.MapPost(RegisterGymFromRequest, "Register/FromRequest").RequireAuthorization();

        //groupBuilder.MapPost(RegisterGym, "Register").RequireAuthorization();
    }

    public async Task<IResult> UpdateMyGymProfile(ISender sender, [FromBody] UpdateMyGymProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Ok<List<GymDto>>> GetAllGyms(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymsQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetGymDetails(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymDetailsQuery(gymId), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Ok<List<GymDto>>> GetNewGymsThisMonth(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNewGymsThisMonthQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> UpdateGymStatus(ISender sender, string gymId, [FromBody] GymStatus newGymStatus, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateGymStatusCommand(gymId, newGymStatus), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Ok<GymDto>> GetMyGymDetails(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> UpdateMyGymStatus(ISender sender, [FromBody] UpdateMyGymStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }
    
    public async Task<IResult> RegisterGymFromRequest(ISender sender, string requestId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterGymFromRequestCommand(requestId));

        return result.ToTypedResult();
    }

    /*
    public async Task<Ok<GymDto>> RegisterGym(ISender sender, [FromBody] RegisterGymCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
    */
}
