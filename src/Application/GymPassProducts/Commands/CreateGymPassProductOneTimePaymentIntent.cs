using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.PaymentIntents.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.User)]
public record CreateGymPassProductOneTimePaymentIntentCommand(
    string GymPassProductId
) : IRequest<Result<PaymentIntentDto>>;

public class CreateGymPassProductOneTimePaymentIntentCommandValidator : AbstractValidator<CreateGymPassProductOneTimePaymentIntentCommand>
{
    public CreateGymPassProductOneTimePaymentIntentCommandValidator()
    {
        RuleFor(v => v.GymPassProductId).NotEmptyWithMessage(nameof(CreateGymPassProductOneTimePaymentIntentCommand.GymPassProductId));
    }
}

public class CreateGymPassProductOneTimePaymentIntentCommandHandler : IRequestHandler<CreateGymPassProductOneTimePaymentIntentCommand, Result<PaymentIntentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentService _paymentService;

    public CreateGymPassProductOneTimePaymentIntentCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentService paymentService
    )
    {
        _context = context;
        _user = user;
        _paymentService = paymentService;
    }

    public async Task<Result<PaymentIntentDto>> Handle(CreateGymPassProductOneTimePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        var product = await _context.GymPassProducts
            .AsNoTracking()
            .Include(p => p.Gym)
            .FirstOrDefaultAsync(p => p.Id == command.GymPassProductId);

        if (product is null)
        {
            return Result.NotFound(nameof(GymPassProduct));
        }

        if (!product.IsActive)
        {
            return Result.BusinessRuleViolation("You cannot buy a pass that is not currently active.");
        }

        if (product.Gym.Status != GymStatus.Active)
        {
            return Result.BusinessRuleViolation("You cannot buy a pass to a gym that is not currently active.");
        }

        var tenantPaymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tpp => tpp.GymId == product.GymId);

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

        return Result.Success(new PaymentIntentDto { ClientSecret = result.Value, TenantPaymentAccountId = tenantPaymentProfile.PaymentAccountId });
    }
}