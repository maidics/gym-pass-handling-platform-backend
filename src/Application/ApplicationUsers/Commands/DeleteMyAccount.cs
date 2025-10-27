using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;
using FitPass.Domain.Strings;
using Microsoft.Extensions.Logging;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize]
public record DeleteMyAccountCommand : IRequest;

public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IStripeCustomerService _stripeCustomerService;
    private readonly ILogger<DeleteMyAccountCommandHandler> _logger;

    public DeleteMyAccountCommandHandler(IIdentityService identityService, IUser user, IStripeCustomerService stripeCustomerService, Logger<DeleteMyAccountCommandHandler> logger)
    {
        _identityService = identityService;
        _user = user;
        _stripeCustomerService = stripeCustomerService;
        _logger = logger;
    }

    public async Task Handle(DeleteMyAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await _identityService.DeleteUserAsync(_user.Id!);

        if (result.IsUserNotFoundFailure())
        {
            _logger.LogError("Authenticated [{Roles}] user ({UserId}) not found.", _user.Roles, _user.Id);
            throw new UnauthorizedAccessException();
        }

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to delete ({UserId}) user. IdentityResult: {IdentityResult}", _user.Id, result);
            throw new InvalidOperationException($"Failed to delete user: '{_user.Id}'.");
        }

        //await _stripeCustomerService.DeleteCustomer(user!);
    }
}
