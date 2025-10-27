using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace FitPass.Application.ApplicationUsers.Commands;

public record ReplaceUserRoleCommand(string OldRole, string NewRole) : IRequest<Result>;

public class ReplaceUserRoleCommandValidator : AbstractValidator<ReplaceUserRoleCommand>
{
    public ReplaceUserRoleCommandValidator()
    {
        RuleFor(v => v.OldRole).IsApplicationRole(nameof(ReplaceUserRoleCommand.OldRole));

        RuleFor(v => v.NewRole).IsApplicationRole(nameof(ReplaceUserRoleCommand.NewRole));
    }
}

public class ReplaceUserRoleCommandHandler : IRequestHandler<ReplaceUserRoleCommand>
{
    private readonly ISender _sender;

    public ReplaceUserRoleCommandHandler(ISender sender)
    {
        _sender = sender;
    }
    
    public Task Handle(ReplaceUserRoleCommand command, CancellationToken cancellationToken)
    {
        if (command.OldRole == Roles.PendingGymManagement && command.NewRole == Roles.GymStaff)
        {
            
        }
    }
}