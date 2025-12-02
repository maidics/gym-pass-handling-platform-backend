using FitPass.Application.ApplicationUsers.Commands;
using FitPass.Application.ApplicationUsers.DTOs;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.Users.Commands;
using FitPass.Application.Users.Commands.Emails;
using FitPass.Application.Users.Commands.RoleHandling;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

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

        groupBuilder.MapPost(RequestPasswordResetEmail, "RequestPasswordResetEmail");

        groupBuilder.MapPost(ResetPassword, "ResetPassword");

        groupBuilder.MapPut(SendEmailConfirmationEmail, "EmailConfirmationEmail").RequireAuthorization();

        groupBuilder.MapPut(ActivateUserAccount, "ActivateAccount");

        groupBuilder.MapPost(GymEmployeeRegisterUser, "Register/ByGymEmployee").RequireAuthorization();

        groupBuilder.MapPut(promote)
    }

    public async Task<IResult> RegisterUser(ISender sender, [FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<IResult> LogInUser(ISender sender, [FromBody] LogInUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<IResult> RegisterPendingGymManagement(ISender sender, [FromBody] RegisterPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<IResult> PromotePendingGymEmployeeToGymStaffRole(ISender sender, [FromBody] PromotePendingGymEmployeeToGymStaffRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<IResult> DeleteMyAccount(ISender sender)
    {
        var result = await sender.Send(new DeleteMyAccountCommand());

        return result.ToTypedResult();
    }

    public async Task<IResult> RequestPasswordResetEmail(ISender sender, [FromBody] RequestPasswordResetEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> ResetPassword(ISender sender, [FromBody] ResetPasswordCommand command)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> DemoteGymStaffToPendingGymEmployee(ISender sender, [FromBody] DemoteGymStaffToPendingGymEmployeeCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> SendEmailConfirmationEmail(ISender sender, [FromBody] SendEmailConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> ActivateUserAccount(ISender sender, [FromBody] ActivateUserAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> GymEmployeeRegisterUser(ISender sender, [FromBody] GymEmployeeRegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<IResult> PromotePendingGymEmployeeToGymAdminFromRequest(ISender sender, [FromBody] PromotePendingGymEmployeeToGymAdminFromRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }
}
