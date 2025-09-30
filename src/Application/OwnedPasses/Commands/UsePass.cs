using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain;

namespace FitPass.Application;

[Authorize]
public record ApplicationUserUsePassCommand(string QrCode, string OwnedPassId) : IRequest;

public class ApplicationUserUsePassCommandValidator : AbstractValidator<ApplicationUserUsePassCommand>
{
    public ApplicationUserUsePassCommandValidator()
    {
        RuleFor(v => v.QrCode)
            .NotEmpty().WithMessage("A QR code must be provided for this.");
    }
}

public class ApplicationUserUsePassCommandHandler : IRequestHandler<ApplicationUserUsePassCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ApplicationUserUsePassCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ApplicationUserUsePassCommand command, CancellationToken cancellationToken)
    {
        var ownedPass = await _context
            .OwnedPasses
            .Include(op => op.UserGymMembership)
            .FirstOrDefaultAsync(pass => pass.Id == command.OwnedPassId, cancellationToken);

        Guard.Against.NotFound(command.OwnedPassId, ownedPass, "Id");
        
        if (_user.Id != ownedPass.UserGymMembership.UserId)
        {
            throw new UnauthorizedAccessException("This pass does not belong to you.");
        }

        var passUseResult = ownedPass.Use();

        if (passUseResult == PassUseResult.Success)
        {
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new BadRequestException("Pass is expired or has no uses left.");
        }
    }
}
