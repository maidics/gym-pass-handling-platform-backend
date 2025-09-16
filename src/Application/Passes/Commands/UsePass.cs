using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain;

namespace FitPass.Application;

[Authorize]
public record UsePassCommand(string QrCode, string OwnedPassId) : IRequest<Result>;

public class UsePassCommandValidator : AbstractValidator<UsePassCommand>
{
    public UsePassCommandValidator()
    {
        RuleFor(v => v.QrCode)
            .NotEmpty().WithMessage("A QR code must be provided for this.");
    }
}

public class UsePassCommandHandler : IRequestHandler<UsePassCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UsePassCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<Result> Handle(UsePassCommand request, CancellationToken cancellationToken)
    {
        var pass = await _context.Passes.FirstOrDefaultAsync(pass => pass.Id == request.OwnedPassId);

        if (pass == null)
        {
            return Result.Failure(["Pass id is not valid."]);
        }
        
        if (_user.Id != pass.UserGymMembership.ApplicationUserId) //TODO: should I throw or have a custom behaviour for this or just return Result.Failure? have option for GymAdmin to use a user's pass?
        {
            return Result.Failure(["This pass does not belong to you."]);
        }

        var passUseResult = pass.Use();

        if (passUseResult == PassUseResult.Success)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        else
        {
            return Result.Failure(["Pass is expired or has no uses left."]);
        }
    }
}
