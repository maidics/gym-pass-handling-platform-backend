using Fitpass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.Commands.Emails;
using FitPass.Application.ApplicationUsers.Commands.RoleHandling;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fitpass.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterUser, "Register");

        groupBuilder.MapPost(LogInUser, "LogIn");

        groupBuilder.MapPost(RegisterPendingGymManagement, "Register/PendingGymManagement");

        groupBuilder.MapPut(PromotePendingGymEmployeeToGymStaffRole, "Promote/GymStaff").RequireAuthorization();

        groupBuilder.MapPut(DemoteGymStaffToPendingGymEmployee, "Demote/GymStaff").RequireAuthorization();

        groupBuilder.MapDelete(DeleteMyAccount, "My/Delete").RequireAuthorization();

        groupBuilder.MapPut(UpdateMyUserProfile, "My/Profile").RequireAuthorization();

        groupBuilder.MapPost(RequestPasswordResetEmail, "RequestPasswordResetEmail");

        groupBuilder.MapPost(ResetPassword, "ResetPassword");

        groupBuilder.MapPut(SendEmailConfirmationEmail, "EmailConfirmationEmail").RequireAuthorization();

        groupBuilder.MapPut(ActivateUserAccount, "ActivateAccount");

        groupBuilder.MapPost(GymEmployeeRegisterUser, "Register/ByGymEmployee").RequireAuthorization();
    }

    public async Task<Ok<JwtToken>> RegisterUser(ISender sender, [FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<JwtToken>> LogInUser(ISender sender, [FromBody] LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<JwtToken>> RegisterPendingGymManagement(ISender sender, [FromBody] RegisterPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<NoContent> PromotePendingGymEmployeeToGymStaffRole(ISender sender, [FromBody] PromotePendingGymEmployeeToGymStaffRoleCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    public async Task<Ok> DeleteMyAccount(ISender sender)
    {
        await sender.Send(new DeleteMyAccountCommand());

        return TypedResults.Ok();
    }

    public async Task<NoContent> UpdateMyUserProfile(ISender sender, UpdateMyUserProfileCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> RequestPasswordResetEmail(ISender sender, [FromBody] RequestPasswordResetEmailCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> ResetPassword(ISender sender, [FromBody] ResetPasswordCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DemoteGymStaffToPendingGymEmployee(ISender sender, [FromBody] DemoteGymStaffToPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }

    public async Task<Results<InternalServerError, NoContent>> SendEmailConfirmationEmail(ISender sender, [FromBody] SendEmailConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return TypedResults.InternalServerError();
        }

        return TypedResults.NoContent();
    }

    public async Task<Ok<JwtToken>> ActivateUserAccount(ISender sender, [FromBody] ActivateUserAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }

    public async Task<Ok<GymMembershipDto>> GymEmployeeRegisterUser(ISender sender, [FromBody] GymEmployeeRegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return TypedResults.Ok(result);
    }
}
