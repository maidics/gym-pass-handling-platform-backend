using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record RejectRequestCommand(string RequestId) : IRequest;

public class RejectRequestCommandValidator : AbstractValidator<RejectRequestCommand>
{
    public RejectRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage(nameof(RejectRequestCommand.RequestId));
    }
}

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand>
{
    private readonly IApplicationDbContext _context;

    public RejectRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectRequestCommand command, CancellationToken cancellationToken)
    {
        var request = await _context
            .Requests
            .FindAsync(command.RequestId);

        Guard.Against.NotFound(command.RequestId, request, nameof(Request));

        request.Status = Domain.Enums.RequestStatus.Rejected;

        //TODO: send event here: RequestRejectedEvent

        await _context.SaveChangesAsync();
    }
}