using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;
using FitPass.Domain.Enums;

namespace FitPass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record UpdateRequestStatusCommand(string RequestId, RequestStatus NewRequestStatus) : IRequest<Result>;

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

public class UpdateRequestStatusCommandHandler : IRequestHandler<UpdateRequestStatusCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateRequestStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateRequestStatusCommand request, CancellationToken cancellationToken)
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