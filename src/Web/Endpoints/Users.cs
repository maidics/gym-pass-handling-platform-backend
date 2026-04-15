using FitPass.Application.Common.Models;
using FitPass.Application.GymMemberships.DTOs;
using FitPass.Application.Requests.Commands.Fulfill;
using FitPass.Application.Users.Commands;
using FitPass.Application.Users.Commands.Emails;
using FitPass.Application.Users.Commands.RoleHandling;
using FitPass.Application.Users.DTOs;
using FitPass.Application.Users.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FitPass.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterUser, "Register");

        groupBuilder.MapPost(LogInUser, "LogIn");

        groupBuilder
            .MapPut(PromotePendingGymEmployeeToGymStaffRole, "Promote/GymStaff")
            .RequireAuthorization();

        groupBuilder
            .MapPut(DemoteGymStaffToPendingGymEmployee, "Demote/GymStaff")
            .RequireAuthorization();

        groupBuilder.MapDelete(DeleteMyAccount, "My/Delete").RequireAuthorization();

        groupBuilder.MapPost(SendPasswordResetEmail, "RequestPasswordResetEmail");

        groupBuilder.MapPost(ResetPassword, "ResetPassword");

        groupBuilder
            .MapPut(SendEmailConfirmationEmail, "EmailConfirmationEmail")
            .RequireAuthorization();

        groupBuilder.MapPut(ActivateUserAccount, "ActivateAccount");

        groupBuilder
            .MapPost(GymEmployeeRegisterUser, "Register/ByGymEmployee")
            .RequireAuthorization();

        groupBuilder
            .MapPut(PromotePendingGymEmployeeToGymAdminFromRequest, "Promote/GymAdmin/Request")
            .RequireAuthorization();

        groupBuilder.MapGet(GetMyUser, "My").RequireAuthorization();

        groupBuilder.MapPut(UpdateMyPassword, "UpdateMyPassword").RequireAuthorization();

        groupBuilder.MapPost(SendAccountActivationEmail, "AccountActivationEmail");
    }

    public async Task<Results<Ok<Jwt>, ProblemHttpResult>> RegisterUser(
        ISender sender,
        [FromBody] RegisterUserCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<Jwt>, ProblemHttpResult>> LogInUser(
        ISender sender,
        [FromBody] LogInUserCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<
        Results<NoContent, ProblemHttpResult>
    > PromotePendingGymEmployeeToGymStaffRole(
        ISender sender,
        [FromBody] PromotePendingGymEmployeeToGymStaffRoleCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> DeleteMyAccount(ISender sender)
    {
        var result = await sender.Send(new DeleteMyAccountCommand(), CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> SendPasswordResetEmail(
        ISender sender,
        [FromBody] string email,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new SendPasswordResetEmailCommand(email), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<Jwt>, ProblemHttpResult>> ResetPassword(
        ISender sender,
        [FromBody] ResetPasswordCommand command
    )
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> DemoteGymStaffToPendingGymEmployee(
        ISender sender,
        [FromBody] DemoteGymStaffToPendingGymEmployeeCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> SendEmailConfirmationEmail(
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(new SendEmailConfirmationEmailCommand(), cancellationToken);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<Jwt>, ProblemHttpResult>> ActivateUserAccount(
        ISender sender,
        [FromBody] ActivateUserAccountCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Results<Ok<GymMembershipDto>, ProblemHttpResult>> GymEmployeeRegisterUser(
        ISender sender,
        [FromBody] GymEmployeeRegisterUserCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<
        Results<NoContent, ProblemHttpResult>
    > PromotePendingGymEmployeeToGymAdminFromRequest(
        ISender sender,
        [FromBody] PromotePendingGymEmployeeToGymAdminFromRequestCommand command
    )
    {
        var result = await sender.Send(command, CancellationToken.None);

        return result.ToTypedResult();
    }

    public async Task<Ok<UserDto>> GetMyUser(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyUserQuery(), cancellationToken);

        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, ProblemHttpResult>> UpdateMyPassword(
        ISender sender,
        [FromBody] UpdateMyPasswordCommand command
    )
    {
        var result = await sender.Send(command);

        return result.ToTypedResult();
    }

    public async Task<Results<NoContent, ProblemHttpResult>> SendAccountActivationEmail(
        ISender sender,
        [FromBody] string email,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new SendAccountActivationEmailCommand(email),
            cancellationToken
        );

        return result.ToTypedResult();
    }
}
