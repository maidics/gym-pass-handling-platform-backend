
using Fitpass.Application.GymPassProductsTemplates.Commands;
using Fitpass.Application.GymPassProductsTemplates.DTOs;
using Fitpass.Application.GymPassProductsTemplates.Queries;
using FitPass.Application.Common.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Fitpass.Web.Endpoints;

public class GymPassProductTemplates : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProductTemplate).RequireAuthorization();

        groupBuilder.MapGet(GetAllGymPassProductTemplates).RequireAuthorization();

        groupBuilder.MapDelete(DeleteGymPassProductTemplate, "{id}").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymPassProductTemplate, "{id}").RequireAuthorization();
    }

    public async Task<Ok<Result>> CreateGymPassProductTemplate(ISender sender, [AsParameters] CreateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<GymPassProductTemplateDto>>> GetAllGymPassProductTemplates(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymPassProductTemplatesQuery { }, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<Result>, BadRequest>> DeleteGymPassProductTemplate(ISender sender, string id, [AsParameters] DeleteGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        if (id != command.GymPassProductTemplateId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<Result>, BadRequest>> UpdateGymPassProductTemplate(ISender sender, string id, [AsParameters] UpdateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        if (id != command.GymPassProductTemplateId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}
