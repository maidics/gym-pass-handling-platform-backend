using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands.Fulfill;

[Authorize(Roles = Roles.AppAdministrator)]
public record FulfillOtherTypeRequestCommand(string RequestId) : IRequest<Result>;

public class FulfillOtherTypeRequestCommandValidator : AbstractValidator<FulfillOtherTypeRequestCommand>
{
    public FulfillOtherTypeRequestCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.RequestId)
            .NotEmpty()
            .WithMessage(
                localizer.GetPropertyOfEntityIsRequired(nameof(SharedResource.Id), nameof(SharedResource.Request)));
    }
}

public class FulfillOtherTypeRequestCommandHandler : IRequestHandler<FulfillOtherTypeRequestCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalizer _localizer;

    public FulfillOtherTypeRequestCommandHandler(IApplicationDbContext context, ILocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }
    
    public async Task<Result> Handle(FulfillOtherTypeRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context.Requests
            .FirstOrDefaultAsync(x => x.Id == command.RequestId, cancellationToken);

        if (request is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        if (request.Type != RequestType.Other)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.InvalidRequestType)));
        }

        if (request.Status != RequestStatus.Submitted)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequestIsNotOpen)));
        }

        request.Status = RequestStatus.Approved;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
