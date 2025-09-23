using Fitpass.Application.GymPassProductsTemplates.Commands;
using Fitpass.Application.GymPassProductsTemplates.DTOs;
using Fitpass.Application.GymPassProductsTemplates.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints;

public class GymPassProductTemplates : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProductTemplate).RequireAuthorization();

        groupBuilder.MapGet(GetAllGymPassProductTemplates).RequireAuthorization();

        groupBuilder.MapDelete(DeleteGymPassProductTemplate, "{gymPassProductTemplateId}").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymPassProductTemplate, "{gymPassProductTemplateId}").RequireAuthorization();
    }

    public async Task<Ok> CreateGymPassProductTemplate(ISender sender, [FromBody] CreateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok<List<GymPassProductTemplateDto>>> GetAllGymPassProductTemplates(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllGymPassProductTemplatesQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, BadRequest>> DeleteGymPassProductTemplate(ISender sender, [FromRoute] string gymPassProductTemplateId, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteGymPassProductTemplateCommand(gymPassProductTemplateId), cancellationToken);

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, BadRequest>> UpdateGymPassProductTemplate(ISender sender, [FromRoute] string gymPassProductTemplateId, [FromBody] UpdateGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        if (gymPassProductTemplateId != command.GymPassProductTemplateId)
        {
            return TypedResults.BadRequest();
        }

        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
