using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymPassProductsTemplates.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record DeleteGymPassProductTemplateCommand(string GymPassProductTemplateId) : IRequest<Result>;

public class DeleteGymPassProductTemplateCommandValidator : AbstractValidator<DeleteGymPassProductTemplateCommand>
{
    public DeleteGymPassProductTemplateCommandValidator()
    {
        RuleFor(v => v.GymPassProductTemplateId).NotEmptyWithMessage("Gym pass product template id");
    }
}

public class DeleteGymPassProductTemplateCommandHandler : IRequestHandler<DeleteGymPassProductTemplateCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteGymPassProductTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _context.GymPassProductTemplates.FindAsync(command.GymPassProductTemplateId);

        if (template == null)
        {
            return Result.Failure(["Gym pass product template not found."]);
        }

        _context.GymPassProductTemplates.Remove(template);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}