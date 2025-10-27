using FitPass.Application.Common.Models;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.ApplicationUsers.Commands.RoleHandling;

public record SwapUserRoleCommand(string UserId, string OldRole, string NewRole) : IRequest<Result>;

public class SwapUserRoleCommandValidator : AbstractValidator<SwapUserRoleCommand>
{
    public SwapUserRoleCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmptyWithMessage(nameof(SwapUserRoleCommand.UserId));

        RuleFor(v => v.OldRole).IsApplicationRole(nameof(SwapUserRoleCommand.OldRole));

        RuleFor(v => v.NewRole).IsApplicationRole(nameof(SwapUserRoleCommand.NewRole));
    }
}

public class SwapUserRoleCommandHandler : IRequestHandler<SwapUserRoleCommand, Result>
{
    private readonly ISender _sender;

    public SwapUserRoleCommandHandler(ISender sender)
    {
        _sender = sender;
    }
    
    public async Task<Result> Handle(SwapUserRoleCommand command, CancellationToken cancellationToken)
    {
        if (command.OldRole == Roles.PendingGymEmployee && command.NewRole == Roles.GymStaff)
        {
            return await _sender.Send(new PromotePendingGymEmployeeToGymStaffRoleCommand(command.UserId), cancellationToken);
        }

        throw new NotImplementedException();
    }
}
