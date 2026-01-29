using FitPass.Application.GymContactInfos.Commands;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class GymContactInfos : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateGymContactInfo).RequireAuthorization();

        groupBuilder.MapPut(UpdateGymContactInfo, "{gymContactInfoId}").RequireAuthorization();

        groupBuilder.MapDelete(DeleteGymContactInfo, "{gymContactInfoId}").RequireAuthorization();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> CreateGymContactInfo(ISender sender,
        [FromBody] CreateGymContactInfoCommand command)
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateGymContactInfo(ISender sender,
        string gymContactInfoId, [FromBody] UpdateGymContactInfoCommand command)
    {
        if (command.GymContactInfoId != gymContactInfoId)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest);
        }
        
        var result = await sender.Send(command, CancellationToken.None);
        
        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> DeleteGymContactInfo(ISender sender,
        string gymContactInfoId)
    {
        var result = await sender.Send(new DeleteGymContactInfoCommand(gymContactInfoId), CancellationToken.None);

        return result.ToTypedResult();
    }
}
