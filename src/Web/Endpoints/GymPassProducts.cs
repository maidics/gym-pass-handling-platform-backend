using FitPass.Application.GymPassProducts.Commands;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymPassProducts : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProduct).RequireAuthorization();
    }

    public async Task<IResult> CreateGymPassProduct(ISender sender, [FromBody] CreateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }
}
