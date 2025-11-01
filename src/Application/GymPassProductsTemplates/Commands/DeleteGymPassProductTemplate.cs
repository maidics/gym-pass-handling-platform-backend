using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;

namespace Fitpass.Application.GymPassProductsTemplates.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record DeleteGymPassProductTemplateCommand(string GymPassProductTemplateId) : IRequest;

public class DeleteGymPassProductTemplateCommandValidator : AbstractValidator<DeleteGymPassProductTemplateCommand>
{
    public DeleteGymPassProductTemplateCommandValidator()
    {
        RuleFor(v => v.GymPassProductTemplateId).NotEmptyWithMessage(nameof(DeleteGymPassProductTemplateCommand.GymPassProductTemplateId));
    }
}

public class DeleteGymPassProductTemplateCommandHandler : IRequestHandler<DeleteGymPassProductTemplateCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteGymPassProductTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteGymPassProductTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _context.GymPassProductTemplates.FindAsync(command.GymPassProductTemplateId);

        Guard.Against.NotFound(command.GymPassProductTemplateId, template, "Id");

        _context.GymPassProductTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
