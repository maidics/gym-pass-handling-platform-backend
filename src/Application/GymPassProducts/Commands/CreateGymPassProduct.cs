using FitPass.Application.Common.Extensions;
using FitPass.Application.Common.Interfaces;
using FitPass.Application.Common.Interfaces.Payment;
using FitPass.Application.Common.Models;
using FitPass.Application.Common.Resources;
using FitPass.Application.Common.Security;
using FitPass.Application.GymPassProducts.DTOs;
using FitPass.Domain.Constants;
using FitPass.Domain.Entities;
using FitPass.Domain.Entities.Payment;
using FitPass.Domain.Enums;
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
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.Name), MaxLength.Name);

        RuleFor(v => v.Description)
            .NotEmptyWithMaxLengthAndMessageLocalized(localizer, nameof(SharedResource.Description), MaxLength.Description);

        When(v => v.PassType == PassType.SingleUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.TotalUses))
                .Equal(1)
                .WithMessage(localizer.Get(nameof(SharedResource.SingleUsePassCanOnlyHaveOneTotalUse)));

            RuleFor(v => v.DaysAfterExpires)
                .Null()
                .WithMessage(localizer.Get(nameof(SharedResource.UseBasedPassTypeCannotHaveExpirationTime)));
        });

        When(v => v.PassType == PassType.MultiUse, () =>
        {
            RuleFor(v => v.TotalUses)
                .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.TotalUses))
                .GreaterThan(1)
                .WithMessage(localizer.Get(nameof(SharedResource.MultiUsePassTypeMustHaveAtLeastTwoUses)));

            RuleFor(v => v.DaysAfterExpires)
                .Null()
                .WithMessage(localizer.Get(nameof(SharedResource.UseBasedPassTypeCannotHaveExpirationTime)));
        });

        When(v => v.PassType == PassType.Unlimited, () =>
        {
            RuleFor(v => v.DaysAfterExpires)
                .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.DaysAfterExpires))
                .GreaterThan(0).WithMessage(localizer.Get(nameof(SharedResource.UnlimitedPassDaysAfterExpiresAtLeastOne)));

            RuleFor(v => v.TotalUses)
                .Null()
                .WithMessage(localizer.Get(nameof(SharedResource.UnlimitedPassTypesCannotHaveUses)));
        });

        RuleFor(v => v.Price)
            .NotEmptyWithMessageLocalized(localizer, nameof(SharedResource.Price));
    }
}

public class CreateGymPassProductCommandHandler : IRequestHandler<CreateGymPassProductCommand, Result<GymPassProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IPaymentProductService _paymentProductService;
    private readonly IPaymentPriceService _paymentPriceService;
    private readonly ILocalizer _localizer;

    public CreateGymPassProductCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IPaymentProductService paymentProductService,
        IPaymentPriceService paymentPriceService,
        ILocalizer localizer)
    {
        _context = context;
        _user = user;
        _paymentProductService = paymentProductService;
        _paymentPriceService = paymentPriceService;
        _localizer = localizer;
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
            return Result.BusinessRuleViolation(_localizer.Get(
                nameof(SharedResource.CannotCreateGymPassProductForGymThatIsSuspended)));
        }
        
        if (tenantPaymentProfile is null)
        {
            return Result.BusinessRuleViolation(_localizer.Get(nameof(SharedResource.RequiresStripeAccount)));
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
