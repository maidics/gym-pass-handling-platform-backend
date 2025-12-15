using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Infrastructure.Localization.Resources;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RejectRequestCommand(string RequestId) : IRequest<Result>;

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
            .FindAsync(command.RequestId);

        if (request is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.Request)));
        }

        request.Status = Domain.Enums.RequestStatus.Rejected;

        //TODO: send event here: RequestRejectedEvent

        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
