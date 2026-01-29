using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.PaymentIntents.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.User)]
public record CreateGymPassProductOneTimePaymentIntentCommand(
    string GymPassProductId
) : IRequest<Result<PaymentIntentDto>>;

public class CreateGymPassProductOneTimePaymentIntentCommandValidator : AbstractValidator<CreateGymPassProductOneTimePaymentIntentCommand>
{
    public CreateGymPassProductOneTimePaymentIntentCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.GymPassProductId)
            .PropertyOfEntityNotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Id), nameof(SharedResource.GymPassProduct));
    }
}

public class CreateGymPassProductOneTimePaymentIntentCommandHandler : IRequestHandler<CreateGymPassProductOneTimePaymentIntentCommand, Result<PaymentIntentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentService _paymentService;
    private readonly ILocalizer _localizer;

    public CreateGymPassProductOneTimePaymentIntentCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentService paymentService,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _paymentService = paymentService;
        _localizer = localizer;
    }

    public async Task<Result<PaymentIntentDto>> Handle(CreateGymPassProductOneTimePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .AsNoTracking()
            .Include(p => p.Gym)
            .FirstOrDefaultAsync(p => p.Id == command.GymPassProductId, cancellationToken);

        if (product is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassProduct)));
        }

        if (!product.IsActive)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.CannotBuyPassThatIsNotActive)));
        }

        if (product.Gym.Status != GymStatus.Active)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.GymIsNotActive), product.Gym.Name));
        }

        var tenantPaymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tpp => tpp.GymId == product.GymId, cancellationToken);

        Guard.Against.Null(tenantPaymentProfile, nameof(TenantPaymentProfile));

        var result = await _paymentService.CreateOneTimePaymentIntent(
            product.Price, 
            _user.Id!,
            product.GymId,
            product.Id, 
            tenantPaymentProfile.PaymentAccountId);

        if (!result.Succeeded)
        {
            return result.ToFailure<PaymentIntentDto>();
        }

        return Result.Success(new PaymentIntentDto(result.Value, tenantPaymentProfile.PaymentAccountId));
    }
}
