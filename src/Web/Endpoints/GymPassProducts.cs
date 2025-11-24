using FitPass.Application.GymPassProducts.Commands;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymPassProducts : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProduct).RequireAuthorization();

        groupBuilder.MapPut(UpdateGymPassProduct, "{gymPassProductId}").RequireAuthorization();

        groupBuilder.MapPut(UpdateGymPassProductActiveStatus, "{gymPassProductId}/Status").RequireAuthorization();
    }

    public async Task<IResult> CreateGymPassProduct(ISender sender, [FromBody] CreateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> UpdateGymPassProduct(ISender sender, string gymPassProductId, [FromBody] UpdateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        if (gymPassProductId != command.GymPassProductId)
        {
            return TypedResults.BadRequest();
        }

        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> UpdateGymPassProductActiveStatus(ISender sender, string gymPassProductId, [FromBody] bool isActive, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateGymPassProductActiveStatusCommand(gymPassProductId, isActive));

        return result.ToTypedResult();
    }
}
