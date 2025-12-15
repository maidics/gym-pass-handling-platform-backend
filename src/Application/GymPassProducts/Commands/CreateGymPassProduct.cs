using FitPass.Application.Common.Constants;
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
using FitPass.Domain.Strings;
using FitPass.Domain.ValueObjects;

namespace FitPass.Application.GymPassProducts.Commands;

[Authorize(Roles = Roles.GymAdministrator)]
public record CreateGymPassProductCommand
    (
        string Name,
        string Description,
        PassType PassType,
        int? TotalUses,
        int? DaysAfterExpires,
        bool IsActive,
        Money Price
    ) : IRequest<Result<GymPassProductDto>>;

public class CreateGymPassProductCommandValidator : AbstractValidator<CreateGymPassProductCommand>
{
    public CreateGymPassProductCommandValidator(ILocalizer localizer)
    {
        RuleFor(v => v.Name)
            .NotEmptyWithMaxLenghtAndMessageLocalized(localizer, LocalizationKeys.Name,  MaxStringLengths.Name);

        RuleFor(v => v.Description)
            .NotEmptyWithMaxLenghtAndMessageLocalized(localizer, LocalizationKeys.Description,  MaxStringLengths.Description);

        When(v => v.PassType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyLocalized(localizer, LocalizationKeys.TotalUses)
                .Equal(1).WithMessage(localizer.Get(LocalizationKeys.SingleUsePassCanOnlyHaveOneTotalUse));

            RuleFor(v => v.DaysAfterExpires)
                .Null().WithMessage(localizer.Get(LocalizationKeys.UseBasedPassTypeCannotHaveExpirationTime));
        });

        When(v => v.PassType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyLocalized(localizer, LocalizationKeys.TotalUses)
                .GreaterThan(1).WithMessage(localizer.Get(LocalizationKeys.MultiUsePassTypeMustHaveAtLeastTwoUses));

            RuleFor(v => v.DaysAfterExpires).Null().WithMessage(ErrorMessages.MultiUsePassCannotExpire());
        });

        When(v => v.PassType == PassType.Unlimited, () =>
        {
            RuleFor(v => v.DaysAfterExpires)
                .NotEmptyLocalized(localizer, LocalizationKeys.DaysAfterExpires)
                .GreaterThan(0).WithMessage(ErrorMessages.UnlimitedPassTypeExpirationDayAtleastOne());

            RuleFor(v => v.TotalUses).Null().WithMessage(ErrorMessages.UnlimitedPassTypeNoUses());
        });

        RuleFor(v => v.Price)
            .NotEmptyLocalized(nameof(CreateGymPassProductCommand.Price));

        RuleFor(v => v.IsActive).NotEmptyLocalized("Active status");
    }
}

public class CreateGymPassProductCommandHandler : IRequestHandler<CreateGymPassProductCommand, Result<GymPassProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentProductService _paymentProductService;
    private readonly IPaymentPriceService _paymentPriceService; 

    public CreateGymPassProductCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentProductService paymentProductService,
        IPaymentPriceService paymentPriceService
    )
    {
        _context = context;
        _user = user;
        _paymentProductService = paymentProductService;
        _paymentPriceService = paymentPriceService;
    }

    public async Task<Result<GymPassProductDto>> Handle(CreateGymPassProductCommand command, CancellationToken cancellationToken)
    {
        var moneyValidationResult = _paymentPriceService.ValidateMoney(command.Price);

        if (!moneyValidationResult.Succeeded)
        {
            return new ResultFailure(moneyValidationResult);
        }
        
        var gymEmployment = await _context.GymEmployments
            .AsNoTracking()
            .Include(x => x.Gym)
            .FirstOrDefaultAsync(ge => ge.UserId == _user.Id);

        Guard.Against.NullParameterRelatedToCurrentUser(gymEmployment, nameof(GymEmployment), _user.Id);

        var tenantPaymentProfile = await _context.TenantPaymentProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.GymId == gymEmployment.GymId);

        if (gymEmployment.Gym.Status == GymStatus.Suspended)
        {
            return Result.BusinessRuleViolation("You cannot create a pass for a gym that is suspended.");
        }
        
        if (tenantPaymentProfile is null)
        {
            return Result.BusinessRuleViolation("You must first create your Stripe payment account.");
        }

        var productResult = await _paymentProductService.CreateProductAsync(
            command.Name, 
            command.Description, 
            command.PassType, 
            command.IsActive,
            tenantPaymentProfile.PaymentAccountId);

        if (!productResult.Succeeded)
        {
            return productResult.ToFailure<GymPassProductDto>();
        }

        var priceResult = await _paymentPriceService.CreatePriceAsync(
            productResult.Value, 
            command.Price, 
            command.IsActive, 
            tenantPaymentProfile.PaymentAccountId);

        if (!priceResult.Succeeded)
        {
            return priceResult.ToFailure<GymPassProductDto>();
        }

        var product = command.PassType switch
        {
            PassType.SingleUse => GymPassProduct.SingleUse(
                gymEmployment.GymId,
                command.Name,
                command.Description,
                command.IsActive,
                command.Price),

            PassType.MultiUse => GymPassProduct.MultiUse(
                gymEmployment.GymId,
                command.Name,
                command.Description,
                (int)command.TotalUses!,
                command.IsActive,
                command.Price),

            PassType.Unlimited => GymPassProduct.UnlimitedUse(
                gymEmployment.GymId,
                command.Name,
                command.Description,
                (int)command.DaysAfterExpires!,
                command.IsActive,
                command.Price),

            _ => throw new NotImplementedException()
        };

        var paymentIdenity = new ProductPaymentIdentity
        {
            ProductId = productResult.Value,
            GymPassProductId = product.Id,
            PriceId = priceResult.Value,
        };

        await _context.GymPassProducts.AddAsync(product);
        await _context.ProductPaymentIdentities.AddAsync(paymentIdenity);
        await _context.SaveChangesAsync();

        return Result.Success(product.MapToDto());
    }
}
