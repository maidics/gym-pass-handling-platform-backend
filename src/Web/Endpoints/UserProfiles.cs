using FitPass.Application.UserProfiles.Queries;
using FitPass.Application.UserProfiles.Commands;
using FitPass.Application.UserProfiles.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FitPass.Web.Endpoints;

public class UserProfiles : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMyUserProfile, "My").RequireAuthorization();

        groupBuilder.MapPut(UpdateMyUserProfile, "My/Update").RequireAuthorization();
        
        groupBuilder.MapPut(UpdateUserProfilePreferredLanguage, "My/Update/PreferredLanguage").RequireAuthorization();
    }

    public async Task<Ok<UserProfileWithEmailDto>> GetMyUserProfile(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyUserProfileQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<IResult> UpdateMyUserProfile(ISender sender, UpdateMyUserProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Ok> UpdateUserProfilePreferredLanguage(ISender sender,
        UpdateUserProfilePreferredLanguageCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command);
        
        return TypedResults.Ok();
    }
}
