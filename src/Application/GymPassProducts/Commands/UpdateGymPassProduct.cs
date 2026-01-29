using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Security;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
using FitPass.Domain.ValueObjects;
using FitPass.Application.Common.Resources;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record UpdateGymPassProductCommand(
    string GymPassProductId,
    PassType Type, //this will not be updated, taking it for validation
    string Name,
    string Description,
    Money Price,
    int? TotalUses,
    int? DaysAfterExpiring
) : IRequest<Result<GymPassProductDto>>;

public class UpdateGymPassProductCommandValidator : AbstractValidator<UpdateGymPassProductCommand>
{
    public UpdateGymPassProductCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Name)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.Name), MaxLengths.Name);

        RuleFor(v => v.Description)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.Description), MaxLengths.Description);
        
        RuleFor(v => v.Price).ValidMoneyWithMessageLocalized(localizer);

        //checking here to validate business rules & checking this in handler if it matches the actual type - if not: BadRequest
        When(v => v.Type == PassType.SingleUse, () => 
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.TotalUses))
                .Must(x => x == 1).WithMessage(localizer.Get(nameof(SharedResource.SingleUsePassCanOnlyHaveOneTotalUse)));

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage(localizer.Get(SharedResource.UseBasedPassTypeCannotHaveExpirationTime));
        });

        When(v => v.Type == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.TotalUses))
                .GreaterThan(1).WithMessage(localizer.Get(nameof(SharedResource.MultiUsePassTypeMustHaveAtLeastTwoUses)))
                .WithMessage(localizer.Get(nameof(SharedResource.UseBasedPassTypeMustHaveOneUse)));

            RuleFor(v => v.DaysAfterExpiring)
                .Null().WithMessage(localizer.Get(nameof(SharedResource.UseBasedPassTypeCannotHaveExpirationTime)));
        });

        When(v => v.Type == PassType.Unlimited, () =>
        {
           RuleFor(v => v.DaysAfterExpiring)
                .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.DaysAfterExpires))
                .GreaterThan(0).WithMessage(localizer.Get(nameof(SharedResource.UnlimitedPassDaysAfterExpiresAtLeastOne)));

           RuleFor(v => v.TotalUses)
               .Null().WithMessage(localizer.Get(nameof(SharedResource.UnlimitedPassTypesCannotHaveUses))); 
        });
    }
}

public class UpdateGymPassProductCommandHandler : IRequestHandler<UpdateGymPassProductCommand, Result<GymPassProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentPriceService _paymentPriceService;
    private readonly IPaymentProductService _paymentProductService;
    private readonly TimeProvider _timeProvider;
    private readonly ILocalizer _localizer;
    
    public UpdateGymPassProductCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentPriceService paymentPriceService,
        IPaymentProductService paymentProductService,
        TimeProvider timeProvider,
        ILocalizer localizer
    )
    {
        _context = context;
        _user = user;
        _paymentPriceService = paymentPriceService;
        _paymentProductService = paymentProductService;
        _timeProvider = timeProvider;
        _localizer = localizer;
    }

    public async Task<Result<GymPassProductDto>> Handle(UpdateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        var moneyValidationResult = _paymentPriceService.ValidateMoney(command.Price);

        if (!moneyValidationResult.Succeeded)
        {
            return new ResultFailure(moneyValidationResult);
        }
        
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .Include(x => x.Gym)
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id, cancellationToken);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        if (gymEmployment.Gym.Status == GymStatus.Suspended)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.YourGymIsSuspended)));
        }

        var tenantPaymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId, cancellationToken);

        Guard.Against.Null(tenantPaymentProfile, nameof(TenantPaymentProfile), "No payment profile found when trying to update GymPassProduct.");

        var product = await _context.GymPassProducts
            .Include(gpp => gpp.PaymentIdentity)
            .FirstOrDefaultAsync(gpp => gpp.Id == command.GymPassProductId, cancellationToken);

        if (product is null)
        {
            return Result.NotFound(_localizer.GetNotFound(nameof(SharedResource.GymPassProduct)));
        }

        if (product.Type != command.Type)
        {
            return Result.BusinessRuleViolation("");
        }

        if (product.Price != command.Price)
        {
            var result = await _paymentPriceService.UpdatePriceAsync( //new price created
                product.PaymentIdentity.PriceId, 
                product.PaymentIdentity.ProductId, 
                command.Price,
                product.IsActive,
                tenantPaymentProfile.PaymentAccountId);

            if (!result.Succeeded)
            {
                return new ResultFailure(result);
            }

            product.PaymentIdentity.ArchivedPaymentProviderPrices.Add(
                new ArchivedPaymentProviderPrice 
                { 
                    Id = product.PaymentIdentity.PriceId, 
                    ArchivedOn = _timeProvider.GetUtcNow() 
                });

            product.Price = command.Price;
            product.PaymentIdentity.PriceId = result.Value;
        }

        if (product.Name != command.Name || product.Description != command.Description)
        {
            var result = await _paymentProductService.UpdateProductAsync(
                product.PaymentIdentity.ProductId, 
                tenantPaymentProfile.PaymentAccountId,
                name: command.Name, 
                description: command.Description);

            if (!result.Succeeded)
            {
                return new ResultFailure(result);
            }

            product.Name = command.Name;
            product.Description = command.Description;
        }

        product
            .UpdateTotalUsesIfApplicable(command.TotalUses)
            .UpdateDaysAfterExpiringIfApplicable(command.DaysAfterExpiring);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(product.MapToDto());
    }
}
