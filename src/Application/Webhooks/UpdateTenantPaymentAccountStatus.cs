using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Strings;

namespace FitPass.Application.Webhooks;

//Webhook only - updated from Stripe
public record UpdateTenantPaymentProfileAccountStatusCommand(
    string TenantAccountId,
    bool DetailsSubmitted, 
    bool ChargesEnabled, 
    bool PayoutsEnabled, 
    List<string> RequirementsDue, 
    List<string> RequirementsEventuallyDue
) : IRequest;

public class UpdateTenantPaymentProfileAccountStatusCommandValidator : AbstractValidator<UpdateTenantPaymentProfileAccountStatusCommand>
{
    public UpdateTenantPaymentProfileAccountStatusCommandValidator()
    {
        RuleFor(v => v.TenantAccountId).NotEmpty();

        RuleFor(v => v.DetailsSubmitted).NotEmpty();

        RuleFor(v => v.ChargesEnabled).NotEmpty();

        RuleFor(v => v.PayoutsEnabled).NotEmpty();

        RuleFor(v => v.RequirementsDue).NotNull();

        RuleFor(v => v.RequirementsEventuallyDue).NotNull();
    }
}

public class UpdateTenantPaymentProfileAccountStatusCommandHandler : IRequestHandler<UpdateTenantPaymentProfileAccountStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantPaymentProfileAccountStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateTenantPaymentProfileAccountStatusCommand command, CancellationToken cancellationToken)
    {
        var tenantPaymentProfile = await _context
            .TenantPaymentProfiles
            .FirstOrDefaultAsync(tpp => command.TenantAccountId == tpp.PaymentAccountId);

        Guard.Against.Null(tenantPaymentProfile, nameof(TenantPaymentProfile));

        var status = new TenantPaymentAccountStatus
        {
            ChargesEnabled = command.ChargesEnabled,
            DetailsSubmitted = command.DetailsSubmitted,
            PayoutsEnabled = command.PayoutsEnabled,
            RequirementsDue = command.RequirementsDue,
            RequirementsEventuallyDue = command.RequirementsEventuallyDue
        };

        tenantPaymentProfile.AccountStatus = status;

        await _context.SaveChangesAsync();
    }
}
