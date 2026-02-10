using FitPass.Application.GymPassProducts.Commands;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Application.GymPassProducts.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymPassProducts : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymPassProduct).RequireAuthorization();

        groupBuilder.MapPut(UpdateGymPassProduct, "{gymPassProductId}").RequireAuthorization();

        groupBuilder
            .MapPut(UpdateGymPassProductActiveStatus, "{gymPassProductId}/Status")
            .RequireAuthorization();

        groupBuilder.MapGet(GetGymPassProductsByGymId, "gyms/{gymId}");

        groupBuilder.MapGet(GetGymPassProductById, "{gymPassProductId}");
    }

    public async Task<Results<Ok<GymPassProductDto>, ProblemHttpResult>> CreateGymPassProduct(
        ISender sender,
        [FromBody] CreateGymPassProductCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<GymPassProductDto>, ProblemHttpResult>> UpdateGymPassProduct(
        ISender sender,
        string gymPassProductId,
        [FromBody] UpdateGymPassProductCommand command
    )
    {
        if (gymPassProductId != command.GymPassProductId)
        {
            return TypedResults.Problem(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Bad Request",
                }
            );
        }

        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateGymPassProductActiveStatus(
        ISender sender,
        string gymPassProductId,
        [FromBody] bool isActive
    )
    {
        var result = await sender.Send(
            new UpdateGymPassProductActiveStatusCommand(gymPassProductId, isActive),
            CancellationToken.None
        );

        return result.ToTypedResult();
    }

    public async Task<
        Results<Ok<List<GymPassProductDto>>, ProblemHttpResult>
    > GetGymPassProductsByGymId(ISender sender, string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetGymPassProductsByGymIdQuery(gymId),
            cancellationToken
        );

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<GymPassProductDto>, ProblemHttpResult>> GetGymPassProductById(
        ISender sender,
        string gymPassProductId,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new GetGymPassProductByIdQuery(gymPassProductId),
            cancellationToken
        );

        return result.ToTypedResult();
    }
}
