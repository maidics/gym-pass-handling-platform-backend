using FitPass.Application.UserProfiles.Commands;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class UserProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(UpdateMyUserProfile, "My/Update").RequireAuthorization();

        groupBuilder
            .MapPut(UpdateMyPreferredLanguage, "My/Update/PreferredLanguage")
            .RequireAuthorization();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateMyUserProfile(
        ISender sender,
        UpdateMyUserProfileCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<NoContent> UpdateMyPreferredLanguage(ISender sender, string newLanguage)
    {
        await sender.Send(
            new UpdateMyPreferredLanguageCommand(newLanguage),
            CancellationToken.None
        );

        return TypedResults.NoContent();
    }
}
