using Fitpass.Application.ApplicationUsers.Commands;
using Fitpass.Application.ApplicationUsers.DTOs;
using Fitpass.Application.ApplicationUsers.Queries;
using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.Gyms.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints.Users;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterUser, "Register");

        groupBuilder.MapPost(LogInUser, "LogIn");

        groupBuilder.MapPost(RegisterPendingGymManagement, "Register/PendingGymManagement");

        groupBuilder.MapGet(GetAllMyGymStaff, "GymManagement/My").RequireAuthorization();

        groupBuilder.MapPut(NominateGymStaff, "Nominate/GymStaff").RequireAuthorization();

        groupBuilder.MapGet(GetMyGymManagementUsers, "GymManagement/My/GymMembers").RequireAuthorization();

        groupBuilder.MapGet(GetGymManagementUsers, "Management/{gymId}").RequireAuthorization();

        groupBuilder.MapDelete(DeleteMyAccount, "My/Delete").RequireAuthorization();

        groupBuilder.MapGet(GetMyUserProfileData, "My/Profile").RequireAuthorization();

        groupBuilder.MapPut(UpdateMyUserProfile, "My/Profile").RequireAuthorization();

        groupBuilder.MapPost(RequestPasswordResetEmail, "My/PasswordReset").RequireAuthorization();
    }

    public async Task<Ok<TokenResponse>> RegisterUser(ISender sender, [FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<TokenResponse>> LogInUser(ISender sender, [FromBody] LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<TokenResponse>> RegisterPendingGymManagement(ISender sender, [FromBody] RegisterPendingGymManagementCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetAllMyGymStaff(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllMyGymStaffQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok> NominateGymStaff(ISender sender, [FromBody] NominateGymStaffCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.Ok();
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetMyGymManagementUsers(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyGymManagementUsersQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<List<ApplicationUserDto>>> GetGymManagementUsers(ISender sender, [FromRoute] string gymId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGymStaffQuery(gymId), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok> DeleteMyAccount(ISender sender)
    {
        await sender.Send(new DeleteMyAccountCommand());

        return TypedResults.Ok();
    }

    public async Task<Ok<ApplicationUserProfileDataDto>> GetMyUserProfileData(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyUserProfileDataQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> UpdateMyUserProfile(ISender sender, UpdateMyUserProfileCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> RequestPasswordResetEmail(ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new RequestPasswordResetEmailCommand());

        return TypedResults.NoContent();
    }
}
