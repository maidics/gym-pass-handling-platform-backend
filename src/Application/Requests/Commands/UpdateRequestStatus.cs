using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record UpdateRequestStatusCommand(string RequestId, RequestStatus NewRequestStatus) : IRequest;

public class UpdateRequestStatusCommandValidator : AbstractValidator<UpdateRequestStatusCommand>
{
    public UpdateRequestStatusCommandValidator()
    {
        List<RequestStatus> allowedStatuses = [
            RequestStatus.InProgress,
            RequestStatus.Cancelled,
            RequestStatus.Rejected,
            RequestStatus.Completed,
        ];

        RuleFor(v => v.NewRequestStatus)
            .NotEmptyWithMessage("New request status")
            .Must(allowedStatuses.Contains);
    }
}

public class UpdateRequestStatusCommandHandler : IRequestHandler<UpdateRequestStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRequestStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateRequestStatusCommand command, CancellationToken cancellationToken)
    {
        var request = await _context.Requests.FindAsync(command.RequestId, cancellationToken);

        Guard.Against.NotFound(command.RequestId, request, "Id");

        request.Status = command.NewRequestStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return;
    }
}