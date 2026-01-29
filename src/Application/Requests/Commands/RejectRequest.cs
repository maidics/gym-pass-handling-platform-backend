using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Application.Common.Resources;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RejectRequestCommand(string RequestId, string? Rationale) : IRequest<Result>;

public class RejectRequestCommandValidator : AbstractValidator<RejectRequestCommand>
{
    public RejectRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.Request));
    }
}

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public RejectRequestCommandHandler(IApplicationDbContext context, ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FindAsync([command.RequestId], cancellationToken);

        if (request is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequestIsNotOpen)));
        }

        request.Status = RequestStatus.Rejected;
        request.HandlerRationale = command.Rationale;
        
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
