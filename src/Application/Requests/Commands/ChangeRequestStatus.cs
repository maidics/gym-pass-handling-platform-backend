using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record ChangeRequestStatusCommand(string RequestId, RequestStatus NewRequestStatus) : IRequest<Result>;

public class ChangeRequestStatusCommandValidator : AbstractValidator<ChangeRequestStatusCommand>
{
    public ChangeRequestStatusCommandValidator()
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

public class ChangeRequestStatusCommandHandler : IRequestHandler<ChangeRequestStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public ChangeRequestStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ChangeRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var requestToChange = await _context.GymCreationRequests.FirstOrDefaultAsync(r => r.Id == request.RequestId);

        if (requestToChange == null)
        {
            return Result.Failure(["Request not found."]);
        }

        requestToChange.RequestStatus = requestToChange.RequestStatus;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}