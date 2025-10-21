using Fitpass.Application.Common.Exceptions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Security;

namespace FitPass.Application.ApplicationUsers.Commands;

[Authorize]
public record DeleteMyAccountCommand : IRequest;

public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _user;
    private readonly IStripeCustomerService _stripeCustomerService;

    public DeleteMyAccountCommandHandler(IIdentityService identityService, IUser user, IStripeCustomerService stripeCustomerService)
    {
        _identityService = identityService;
        _user = user;
        _stripeCustomerService = stripeCustomerService;
    }

    public async Task Handle(DeleteMyAccountCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindUserByIdAsync(_user.Id!);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        var result = await _identityService.DeleteUserAsync(user!);

        if (!result.Succeeded)
        {
            throw new BadRequestException(string.Join(", ", result.Errors));
        }

        await _stripeCustomerService.DeleteCustomer(user!);
    }
}