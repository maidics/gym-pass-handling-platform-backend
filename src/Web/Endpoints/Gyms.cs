using Fitpass.Application.ApplicationUsers.Queries;
using Fitpass.Application.Gyms.Commands;
using Fitpass.Application.Gyms.DTOs;
using Fitpass.Application.Gyms.Queries;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Common.Models;
using FitPass.Application.Gyms.Commands;
using FitPass.Application.Gyms.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class Gyms : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterGym, "Register/{requestId}").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymQrCode, "My/QrCode").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymProfile, "Profile/{id}").RequireAuthorization();

        groupBuilder.MapGet(GetAllGyms).RequireAuthorization();

        groupBuilder.MapGet(GetGymDetails, "Details/{id}").RequireAuthorization();

        groupBuilder.MapGet(GetNewGymsThisMonth, "NewThisMonth").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymStatus, "Status/{id}").RequireAuthorization();

        groupBuilder.MapGet(GetGymManagementUsers, "Management").RequireAuthorization(); //TODO: split this endpoint into two: GetGymManagementUsers (AppAdmin), GetMyGymManagementUsers (Gym management)
    }

    public async Task<Results<Ok<Result>, BadRequest>> RegisterGym(ISender sender, string requestId, [AsParameters] RegisterGymCommand command)
    {
        if (requestId != command.gymCreationRequestId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> GetMyGymQrCode(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymQrCodeQuery { }, cancellationToken);

        return TypedResults.File(result, contentType: "image/png", fileDownloadName: "gymQrCode.png");
    }

    public async Task<Results<Ok<GymDto>, NotFound>> UpdateGymProfile(ISender sender, string id, [AsParameters] UpdateGymProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymDto>>> GetAllGyms(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymsQuery { }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<GymDto>, NotFound, BadRequest>> GetGymDetails(ISender sender, string id, [AsParameters] GetGymDetailsQuery query, CancellationToken cancellationToken)
    {
        if (id != query.GymId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(query, cancellationToken);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymDto>>> GetNewGymsThisMonth(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNewGymsThisMonthQuery { }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateGymStatus(ISender sender, string id, UpdateGymStatusCommand command, CancellationToken cancellationToken)
    {
        if (id != command.GymID)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<List<ApplicationUserDto>>, BadRequest<string>>> GetGymManagementUsers(ISender sender, [AsParameters] GetGymStaffQuery query, CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);

        if (result.errorMessage != null)
        {
            return TypedResults.BadRequest(result.errorMessage);
        }

        return TypedResults.Ok(result.gymStaffManagementUsers);
    }

    public async Task<Ok<ApplicationUserDto>> GetMyGymManagementUsers(ISender sender, [AsParameters])
}
