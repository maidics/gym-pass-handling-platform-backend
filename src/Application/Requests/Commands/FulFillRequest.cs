using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Application.Extensions;
using FitPass.Domain.Constants;

namespace Fitpass.Application.Requests.Commands;

[Authorize(Roles = Roles.AppAdministrator)]
public record FulfillRequestCommand
    (
        string RequestId
    ) : IRequest;

public class FulfillRequestCommandValidator : AbstractValidator<FulfillRequestCommand>
{
    public FulfillRequestCommandValidator()
    {
        RuleFor(v => v.RequestId).NotEmptyWithMessage("Request id");
    }
}

public class FulfillRequestCommandHandler : IRequestHandler<FulfillRequestCommand>
{
    private readonly IRequestService _requestService;

    public FulfillRequestCommandHandler(IRequestService requestService)
    {
        _requestService = requestService;
    }

    public async Task Handle(FulfillRequestCommand command, CancellationToken cancellationToken)
    {
        await _requestService.FulfillRequest(command.RequestId);
    }
}